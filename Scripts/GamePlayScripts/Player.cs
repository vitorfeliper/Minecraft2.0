using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    TitleMenu settings;

    private Transform cam;
    private World world;
    private bool _inS = false;
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;

    private Vector3 velocity;
    private float verticalMomentum = 0f;
    private bool jumpRequest;

    public Transform highlightBlock;
    public Transform placeBlock;

    public float checkIncrement = 0.1f;
    public float reach = 8f;

    //public Text selectedBlockText;
    //public byte selectedBlockIndex = 1;

    public ToolBar toolBar;

    public GameObject screen;
    public GameObject configs;
    bool isScreen;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        world.inUI = false;

        //selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName + " Block selected";
    }

    private void FixedUpdate()
    {
        if (!world.inUI)
        {
            CalculateVelocity();
            if (jumpRequest)
            {
                Jump();
            }
            transform.Translate(velocity, Space.World);
        }

    }

    public void FPSControl()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isScreen = !isScreen;
            if (isScreen)
            {
                ScreenOn();
            }
            else
            {
                ScreenOff();
            }
        }
    }

    public void ScreenOn()
    {
        screen.SetActive(true);
    }
    public void ScreenOff()
    {
        screen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            world.inUI = !world.inUI;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inS = !inS;
        }


        if (!world.inUI || !inS)
        {
            GetPlayerInputs();
            //FPSControl();
            PlaceCursorBlocks();
        }

        transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity);
        cam.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSensitivity);
    }

    public bool inS
    {
        get { return _inS; }
        set
        {
            _inS = value;
            if (_inS)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                configs.SetActive(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                configs.SetActive(false);
            }
        }
    }

    public void Done()
    {
        settings.LeaveSettings2();

        configs.SetActive(false);
    }

    public void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }


    private void CalculateVelocity()
    {
        //Affect vertical momentum  with gravity
        if(verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }

        //If we're sprinting, use the sprinting multipler
        if (isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }

        //Apply vertical momentum(falling/jump)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if((velocity.z > 0 && Front) || (velocity.z < 0 && Back))
        {
            velocity.z = 0;
        }

        if ((velocity.x > 0 && Right0) || (velocity.x < 0 && Left0))
        {
            velocity.x = 0;
        }

        if(velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
    }

    private void GetPlayerInputs()
    {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }

        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }


        if (highlightBlock.gameObject.activeSelf)
        {
            //Destroy block
            if (Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }

            //Place block
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 playerCoord = new Vector3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
                if (playerCoord != placeBlock.position && (playerCoord + new Vector3(0, 1, 0)) != placeBlock.position)
                {
                    Vector3 playerCoord0 = new Vector3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
                    if (playerCoord0 != placeBlock.position && (playerCoord0 + new Vector3(1, 0, 0)) != placeBlock.position)
                    {
                        Vector3 playerCoord1 = new Vector3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), Mathf.FloorToInt(transform.position.z));
                        if (playerCoord1 != placeBlock.position && (playerCoord1 + new Vector3(0, 0, 1)) != placeBlock.position)
                        {
                            if (toolBar.slots[toolBar.slotIndex].HasItem)
                            {
                                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolBar.slots[toolBar.slotIndex].itemSlot.stack.id);
                                toolBar.slots[toolBar.slotIndex].itemSlot.Take(1);
                            }
                        }
                    }
                }
            }
        }
    }

    private void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while(step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) || 
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) || 
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) || 
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           )
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
           )
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool Front
    {
        get
        {
            if (
                    world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                    world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Back
    {
        get
        {
            if (
                    world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                    world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Left0
    {
        get
        {
            if (
                    world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                    world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Right0
    {
        get
        {
            if (
                    world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                    world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
               )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
