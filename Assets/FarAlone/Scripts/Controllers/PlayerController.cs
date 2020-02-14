﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.UI;
using InjectorGames.FarAlone.UI;

namespace InjectorGames.FarAlone.Players
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public sealed class PlayerController : MonoBehaviour
    {
        #region Singletone
        public static PlayerController Instance;

        private void SetInstance()
        {
            if (Instance != null)
                throw new Exception("Multiple instances of this class is not supported");
            Instance = this;
        }
        #endregion

        public float HP;
        
        /* Movement & stamina variables */
        public float walkingSpeed = 1f;
        public float runningSpeed = 2f;

        public float currentSpeed;

        private float runningTime = 0f;
        private float maxRunningTime = 8f;
        private float restingTime = 0f;
        private float maxRestingTime = 8f;
        public bool canRun = true;

        /* end */

        public float cameraSpeed = 1f;
        public Transform handLeft;
        public Transform blastSpawnPoint;
        public GameObject blastPreafab;

        private float shootDelay;
        private Rigidbody2D rb;
        private Animator animator;
        private Transform HeBarChild;

        private void Awake()
        {
            SetInstance();
        }

        private void Start()
        {
            shootDelay = 0f;
            rb = GetComponent<Rigidbody2D>() ?? throw new NullReferenceException();
            animator = GetComponent<Animator>() ?? throw new NullReferenceException();

            if (handLeft == null)
                throw new NullReferenceException();

            var camera = Camera.main;
            camera.transparencySortMode = TransparencySortMode.CustomAxis;
            camera.transparencySortAxis = new Vector3(0.0f, 1.0f, 0.0f);

            HeBarChild = this.gameObject.transform.GetChild(7);
        }

        private void Update()
        {
            // TODO: open pause menu instead
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit(0);

            UpdateShooting();
        }

        private void FixedUpdate()
        {
            UpdateMovement();
            UpdateCameraFollow();
            UpdateDirection();
            UpdateHandRotation();
            UpdateWalkAnimation();
        }

        private void UpdateMovement()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.LeftShift) && canRun) {
                currentSpeed = runningSpeed;
                runningTime += Time.deltaTime;
                if (runningTime >= maxRunningTime) {
                    runningTime = 0f;
                    canRun = false;
                }
            }
            else {
                currentSpeed = walkingSpeed;
                if (restingTime < maxRestingTime) {
                    restingTime += Time.deltaTime;
                }
                else {
                    restingTime = 0f;
                    canRun = true;
                }
            }

            if (horizontal * vertical == 0)
                rb.velocity = new Vector2(horizontal * currentSpeed, vertical * currentSpeed);
            else
                rb.velocity = new Vector2(horizontal * (currentSpeed / 1.414f), vertical * (currentSpeed / 1.414f));
        }
        private void UpdateCameraFollow()
        {
            var cameraTransform = Camera.main.transform;
            var newPosition = Vector3.Lerp(cameraTransform.position, transform.position, cameraSpeed);
            newPosition.z = -1f;
            cameraTransform.position = newPosition;
        }
        private void UpdateDirection()
        {
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (transform.position.x > mouseWorldPosition.x)
            {
                for(int i = 0; i < transform.childCount; i++)
                {
                    if(transform.GetChild(i).tag != "Status Bar")
                    {
                        transform.eulerAngles = new Vector3(0f, 0f, 0f);
                    }
                }
            }
            else
            {
                for(int i = 0; i < transform.childCount; i++)
                {
                    if(transform.GetChild(i).tag != "Status Bar")
                    {
                        transform.eulerAngles = new Vector3(0f, 180f, 0f);
                    }
                }
            }
        }
        private void UpdateHandRotation()
        {
            var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var relative = handLeft.InverseTransformPoint(mouseWorldPosition);
            var angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
            handLeft.Rotate(0, 0, -angle);
        }
        private void UpdateWalkAnimation()
        {
            if (rb.velocity.magnitude > 0f)
            {
                animator.SetBool("IsWalking", true);

                if (currentSpeed == runningSpeed)
                    animator.speed = runningSpeed;
                else
                    animator.speed = walkingSpeed;
            }
            else
            {
                animator.SetBool("IsWalking", false);
            }
        }

        private void UpdateShooting()
        {
            shootDelay -= Time.deltaTime;

            if (shootDelay < 0f && Input.GetAxis("Fire1") == 1.0f)
            {
                shootDelay = 0.25f;

                // TODO: move shooting offset to weapon specification
                var offset = new Vector3(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(-20, 20));
                var direction = Camera.main.ScreenToWorldPoint(Input.mousePosition + offset) - blastSpawnPoint.position;

                if (direction.magnitude > 1.414f)
                {
                    var blast = Instantiate(blastPreafab);
                    var blastTransform = blast.transform;
                    blastTransform.position = blastSpawnPoint.position;
                    blastTransform.rotation = blastSpawnPoint.rotation;

                    var blastRigidbody = blast.GetComponent<Rigidbody2D>();
                    blastRigidbody.velocity = direction.normalized * 10f;

                    Destroy(blast, 5f);
                }
            }
        }
    }
}