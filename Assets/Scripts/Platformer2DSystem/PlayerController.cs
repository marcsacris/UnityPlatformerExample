using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace Platformer2DSystem.Example
{
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(Runner))]
    [RequireComponent(typeof(Jumper))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private int jumpBufferFrames = 5;
        [SerializeField] private float jumpMovementMultiplier = 0.8f;
        [SerializeField] private float doubleJumpMultiplier = 0.9f;
        [SerializeField] private AudioClip jumpSound;


        [Header("Lives Settings")]
        private int lives = 3;
        [SerializeField] private int maxLives = 3;
        [SerializeField] private int enemyCooldown = 100;
        private int enemyCooldownCount;
        private bool isHit = false;
        [SerializeField] private Slider slider;


        [Header("Coins Settings")]
        private int coins = 0;
        [SerializeField] private TextMeshProUGUI coinsText;


        private Actor actor;
        private Runner runner;
        private Jumper jumper;
        private AudioSource audioSource;


        private int remainingJumps;
        private Timer jumpBufferTimer;


        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction downAction;
        private InputAction toggleVictoryAction;


        private Vector2 moveInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool downPressed;

        private GameObject finishText;


        private void Awake()
        {
            actor = GetComponent<Actor>();
            runner = GetComponent<Runner>();
            jumper = GetComponent<Jumper>();
            audioSource = GetComponent<AudioSource>();

            lives = maxLives;

            jumpBufferTimer = Timer.Frames(jumpBufferFrames);
            remainingJumps = maxJumps;

            // Encontrar el texto con tag "Finish" y ocultarlo
            finishText = GameObject.FindWithTag("Finish");
            if (finishText != null)
                finishText.SetActive(false);


            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s");


            moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            moveAction.canceled += ctx => moveInput = Vector2.zero;


            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            jumpAction.performed += ctx => jumpPressed = true;
            jumpAction.canceled += ctx => jumpHeld = false;


            downAction = new InputAction("Down", InputActionType.Button, "<Keyboard>/s");
            downAction.performed += ctx => downPressed = true;
            downAction.canceled += ctx => downPressed = false;

            // Definir la acción para pulsar la tecla I
            toggleVictoryAction = new InputAction("ToggleVictory", InputActionType.Button, "<Keyboard>/i");
            toggleVictoryAction.performed += ctx =>
            {
                if (finishText != null)
                    finishText.SetActive(!finishText.activeSelf); // alterna visibilidad
            };
        }



        private void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            downAction.Enable();
            toggleVictoryAction?.Enable();

            actor.GroundEntered += OnGroundEntered;
            actor.CeilingHit += OnCeilingHit;
            jumper.jumpedGrounded.AddListener(PlayJumpSound);
            jumper.jumpedAirborne.AddListener(PlayJumpSound);
        }


        private void OnDisable()
        {
            moveAction.Disable();
            jumpAction.Disable();
            downAction.Disable();
            toggleVictoryAction?.Disable();

            actor.GroundEntered -= OnGroundEntered;
            actor.CeilingHit -= OnCeilingHit;
            jumper.jumpedGrounded.RemoveListener(PlayJumpSound);
            jumper.jumpedAirborne.RemoveListener(PlayJumpSound);
        }


        private void Update()
        {
            //Debug.Log(actor.velocity.y);


            if (isHit)
            {
                enemyCooldownCount++;


                if (enemyCooldownCount >= enemyCooldown)
                {
                    enemyCooldownCount = 0;
                    isHit = false;
                }
            }


            UpdateMovement();
            UpdateJumping();
        }


        // --- MOVEMENT ---
        private void UpdateMovement()
        {
            float dirX = moveInput.x;


            if (Mathf.Abs(dirX) < 0.01f)
            {
                runner.Stop();
                return;
            }


            float multiplier = jumper.IsJumping ? jumpMovementMultiplier : 1f;



            runner.Move(dirX, multiplier);
        }


        // --- JUMPING ---
        private void UpdateJumping()
        {
            if (jumpPressed)
            {
                jumpBufferTimer.Start();
                jumpPressed = false;
                jumpHeld = true;
            }


            if (jumpBufferTimer.IsRunning && remainingJumps > 0)
            {
                if (downPressed && actor.IsOnGroundOneWay)
                {
                    jumper.JumpDown();
                }
                else
                {
                    bool isDoubleJump = remainingJumps < maxJumps;
                    float multiplier = isDoubleJump ? doubleJumpMultiplier : 1f;
                    jumper.Jump(multiplier);
                    remainingJumps--;
                }


                jumpBufferTimer.Stop();
            }


            if (!jumpHeld && jumper.IsJumping)
            {
                jumper.CancelJump();
            }
        }


        public void OnGroundEntered(Collider2D ground)
        {
            remainingJumps = maxJumps;
        }


        public void OnCeilingHit(Collider2D ceiling)
        {
            jumper.CancelJump();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.tag == "Portal")
            {
                collision.gameObject.GetComponent<Portal>().GoToScene();
            }

            if (collision.transform.tag == "Coin")
            {
                Destroy(collision.gameObject);

                coins++;
                if (coinsText != null)
                    coinsText.text = coins.ToString();
            }

            if (collision.transform.tag == "Llave")
            {
                Destroy(collision.gameObject);

                // Revelar tesoros y portales ocultos mediante el manager
                if (TreasurePortalManager.Instance != null)
                {
                    TreasurePortalManager.Instance.RevealAll();
                }
                else
                {
                    // Fallback: intentar activar por tag
                    GameObject[] treasures = GameObject.FindGameObjectsWithTag("Tesoro");
                    foreach (GameObject treasure in treasures) treasure.SetActive(true);
                    GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");
                    foreach (GameObject portal in portals) portal.SetActive(true);
                }
            }

            if (collision.transform.tag == "Enemy")
            {
                if (actor.velocity.y < 0) // Pisando al enemigo desde arriba
                {
                    jumper.Jump(doubleJumpMultiplier);
                    remainingJumps--;
                   
                    if (collision.transform.parent != null)
                    {
                        SmallBeeController bee = collision.transform.parent.GetComponent<SmallBeeController>();
                        if (bee != null)
                        {
                            bee.Die();
                        }
                        else
                        {
                            Destroy(collision.transform.parent.gameObject);
                        }
                    }
                    else
                    {
                        Destroy(collision.gameObject);
                    }
                    isHit = true;
                }
                else // Choque lateral o desde abajo
                {
                    if (!isHit)
                    {
                        lives--;
                        if (slider != null)
                            slider.value = (float)lives / (float)maxLives;
                        
                        actor.velocity.x =
                            transform.position.x > collision.transform.position.x ?
                            50f : -50f;

                        isHit = true;
                    }

                    if (lives < 0)
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                }
            }
        }

        public void TakeDamage()
        {
            if (!isHit)
            {
                lives--;
                if (slider != null)
                    slider.value = (float)lives / (float)maxLives;

                isHit = true;

                if (lives < 0)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        public bool IsHit()
        {
            return isHit;
        }

        private void PlayJumpSound()
        {
            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }
    }
}