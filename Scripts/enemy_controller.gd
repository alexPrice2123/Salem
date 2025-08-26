extends CharacterBody3D

##CONSTANTS##
const WALK_SPEED = 7.0 #player movement speed
const SPRINT_SPEED = 14.0 #player sprint speed
const JUMP_VELOCITY = 12.0 #player jump power
const SENSE = 0.01 #mouse sensitivity ##might make a var for settings to change
const BOB_FREQ = 1.5 #camera bobbing frequence
const BOB_AMP = 0.06 #camera bobbing amplitude
const GRAVITY = Vector3(0,25.0,0) #gravity force pulling you down
const BASE_FOV = 80.0 #starting FOV
const FOV_CHANGE = 1.2 #fov multiplier
const DASH_POWER = 15.0
const MAX_HEALTH = 10.0

##VARIABLES##
var handpos = null

var cam_offset = 0.3 #camerashake offset (using shake amount value)
var shake_amount = 0.2 #amount the camera shakes by
var t_bob = 0.0 #camera bobbing current offset value
var speed = WALK_SPEED #setting speed to the const walkspeed so we can change speed
var shook = false #should the camera be chaking or going back to normal
var instance #creates instance variable to use later
var dash_offset = 0.0 #adds this value to your speed when you dash
var FOVOffset = 0.0 #subtracts this value from your FOV when you zoom
var health = MAX_HEALTH
var viewport = null
var mouse_position = null
var camera = null
var origin = null
var direction = null

##COMPONANTS##
@onready var head = $Head
@onready var cam = $Head/Camera3D
@onready var sound = $Sound
@onready var hand = $Hand
@onready var hand_copy = $Head/Camera3D/HandCopy
@onready var ui_path = null

##BEGIN_PLAY##
func _ready() -> void:
	get_tree().paused = false
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	handpos = hand_copy.position
	ui_path = get_tree().get_current_scene().get_node("MainUI").get_node("Control")

##ON_INPUT_RECIVED##
func _unhandled_input(event: InputEvent) -> void:
	#Camera Controls#
	if event is InputEventMouseMotion and Input.mouse_mode == (Input.MOUSE_MODE_CAPTURED):
		head.rotate_y(-event.relative.x * SENSE)
		cam.rotate_x(-event.relative.y * SENSE)
		cam.rotation.x = clamp(cam.rotation.x, deg_to_rad(-80), deg_to_rad(80)) #clamps camera rotation

##EVENT_TICK##
func _physics_process(delta: float) -> void:
	#Gravity Handling#
	if not is_on_floor(): #if the player isnt touching the ground then reduce their upward velocity by the gravity amount
		velocity -= GRAVITY * delta

	#Jump Handling#
	if Input.is_action_just_pressed("jump") and is_on_floor(): #increases the height the player is jumping by adding velocity if the player is still holding down space
		velocity.y = JUMP_VELOCITY
		
	hand.global_transform = lerp(hand.global_transform, hand_copy.global_transform, delta * 25.0)


	#Unlock Mouse#
	if Input.is_action_just_released("unlock_mouse"): #unlocks the mouse from the center of your screen
		if Input.mouse_mode == (Input.MOUSE_MODE_CAPTURED):
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

	#Sprint Handling#
	if Input.is_action_pressed("sprint"): #speeds up the player then they are holding down shift
		speed = SPRINT_SPEED + dash_offset
	else:
		speed = WALK_SPEED + dash_offset
	
	#Movement Handling#
	var input_dir := Input.get_vector("move_left", "move_right", "move_forward", "move_back") #returns different vectors for every input
	var direction = (head.transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized() #gets the direction the player is looking at
	if is_on_floor(): #allows controlled player movment when on the ground
		if direction: #chainges where the player runs depedning on where they look
			velocity.x = direction.x * speed
			velocity.z = direction.z * speed
		else: #adds a little bit of slidding when the player stops moving
			velocity.x = lerp(velocity.x, direction.x * speed, delta * 7.0)
			velocity.z = lerp(velocity.z, direction.z * speed, delta * 7.0)
	else: #makes the player slightly less controllable in the air
		velocity.x = lerp(velocity.x, direction.x * speed, delta * 5.0)
		velocity.z = lerp(velocity.z, direction.z * speed, delta * 5.0)

	#Dash Handling#
	if Input.is_action_just_pressed("dash") and dash_offset <= 1.0:
		dash_offset = DASH_POWER
	if dash_offset > 0:
		dash_offset = lerp(dash_offset, 0.0, delta * 5.0)
	#if dash_offset > 5.0: ##Dash Line effects; revisit later
		#speed_lines.material.set_shader_parameter("line_density", dash_offset/7.0)
	#else:
		#speed_lines.material.set_shader_parameter("line_density", dash_offset/15.0)
	#Camerabob Handling#
	if dash_offset <= 1.0:
		t_bob += delta * velocity.length() * (float(is_on_floor()) + 0.2)
		cam.transform.origin = headbob(t_bob)
		hand_copy.transform.origin = handbob(t_bob)

	#FOV Handling#
	var velocity_clamped = clamp(velocity.length(), 0.5, SPRINT_SPEED * 3)
	var target_fov = BASE_FOV + FOV_CHANGE * velocity_clamped
	cam.fov = lerp(cam.fov, target_fov-FOVOffset, delta * 8.0)
	
	#Move and Slide Func#
	move_and_slide()


####CUSTOM_FUNCTIONS####
func headbob(time) -> Vector3: #bobs the camera up and down using sin and side to side using cos, returns the camera position offset
	var pos = Vector3.ZERO
	pos.y = sin(time * BOB_FREQ) * BOB_AMP
	pos.x = cos(time * BOB_FREQ/2) * BOB_AMP
	return pos

func handbob(time) -> Vector3: #bobs the gun up and down using sin and side to side using cos, returns the gun position offset
	handpos.y = (sin(time * BOB_FREQ/1) * BOB_AMP)
	handpos.x = (cos(time * BOB_FREQ/2) * BOB_AMP)-0.1
	return handpos

func hit(damage):
	health -= damage
	if health <= 0:
		get_tree().paused = true
		get_tree().reload_current_scene()
