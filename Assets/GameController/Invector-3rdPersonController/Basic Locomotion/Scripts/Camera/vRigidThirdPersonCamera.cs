using UnityEngine;
using Invector.vCamera;
using Invector.vCharacterController;

namespace Invector
{
    public class vRigidThirdPersonCamera : vThirdPersonCamera
    {
        [SerializeField] private float distance = 3f; // Текущая дистанция до цели
        [SerializeField] private bool useCollision = true; // Включить проверку столкновений
        [SerializeField] private float collisionOffset = 0.2f; // Отступ от препятствий

        private Quaternion targetRotation;
        private Vector3 targetPosition;

        protected override void Start()
        {
            base.Start();
            distance = currentState.defaultDistance;
            targetRotation = Quaternion.Euler(mouseY, mouseX, 0);
        }

        protected virtual void LateUpdate()
        {
            if (mainTarget == null || targetCamera == null || !isInit || isFreezed)
            {
                return;
            }

            switch (currentState.cameraMode)
            {
                case TPCameraMode.FreeDirectional:
                    UpdateMouseOrbit();
                    break;
                case TPCameraMode.FixedAngle:
                    UpdateFixedAngle();
                    break;
                case TPCameraMode.FixedPoint:
                    CameraFixed();
                    break;
            }
        }

        protected virtual void UpdateMouseOrbit()
        {
            // Обновление состояния камеры
            if (useSmooth)
            {
                currentState.Slerp(lerpState, smoothBetweenState * Time.deltaTime);
            }
            else
            {
                currentState.CopyState(lerpState);
            }

            // Обработка зума
            if (currentState.useZoom)
            {
                distance = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
            }
            else
            {
                distance = currentState.defaultDistance;
                currentZoom = currentState.defaultDistance;
            }

            // Обновление ввода мыши
            if (!lockTarget && !currentState.cameraMode.Equals(TPCameraMode.FixedAngle))
            {
                mouseX += Input.GetAxis("Mouse X") * currentState.xMouseSensitivity * (vInput.instance.inputDevice == InputDevice.Joystick ? joystickSensitivity : 1f);
                mouseY -= Input.GetAxis("Mouse Y") * currentState.yMouseSensitivity * (vInput.instance.inputDevice == InputDevice.Joystick ? joystickSensitivity : 1f);
                mouseY = vExtensions.ClampAngle(mouseY, currentState.yMinLimit, currentState.yMaxLimit);
                mouseX = vExtensions.ClampAngle(mouseX, currentState.xMinLimit, currentState.xMaxLimit);
            }

            // Обновление позиции цели
            targetPosition = currentTarget.position + currentTarget.up * (offSetPlayerPivot + currentState.height);
            targetCamera.fieldOfView = currentState.fov;

            // Обработка вращения
            targetRotation = Quaternion.Euler(mouseY + offsetMouse.y, mouseX + offsetMouse.x, 0);

            // Вычисление желаемой позиции камеры
            Vector3 desiredPosition = targetPosition - (targetRotation * Vector3.forward * distance);

            // Проверка столкновений
            if (useCollision)
            {
                RaycastHit hitInfo;
                Vector3 dir = desiredPosition - targetPosition;
                float dist = dir.magnitude;
                if (Physics.Raycast(targetPosition, dir.normalized, out hitInfo, dist + collisionOffset, cullingLayer))
                {
                    desiredPosition = targetPosition + dir.normalized * (hitInfo.distance - collisionOffset);
                }
            }

            // Установка позиции и вращения камеры
            transform.position = desiredPosition;
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position + currentState.rotationOffSet);

            // Обработка lockTarget
            if (lockTarget)
            {
                var collider = lockTarget.GetComponent<Collider>();
                if (collider != null)
                {
                    Vector3 lockPoint = collider.bounds.center + Vector3.up * heightOffset;
                    Quaternion lockRotation = Quaternion.LookRotation(lockPoint - transform.position);
                    transform.rotation = lockRotation;
                    mouseX = lockRotation.eulerAngles.y;
                    mouseY = lockRotation.eulerAngles.x;
                }
            }

            // Обновление targetLookAt
            targetLookAt.position = targetPosition;
            targetLookAt.rotation = targetRotation;
        }

        protected virtual void UpdateFixedAngle()
        {
            // Обновление состояния камеры
            if (useSmooth)
            {
                currentState.Slerp(lerpState, smoothBetweenState * Time.deltaTime);
            }
            else
            {
                currentState.CopyState(lerpState);
            }

            // Установка фиксированного угла
            mouseX = currentState.fixedAngle.x;
            mouseY = currentState.fixedAngle.y;
            distance = currentState.defaultDistance;

            // Обновление позиции цели
            targetPosition = currentTarget.position + currentTarget.up * (offSetPlayerPivot + currentState.height);
            targetCamera.fieldOfView = currentState.fov;

            // Вычисление вращения
            targetRotation = Quaternion.Euler(mouseY + offsetMouse.y, mouseX + offsetMouse.x, 0);

            // Вычисление желаемой позиции камеры
            Vector3 desiredPosition = targetPosition - (targetRotation * Vector3.forward * distance);

            // Проверка столкновений
            if (useCollision)
            {
                RaycastHit hitInfo;
                Vector3 dir = desiredPosition - targetPosition;
                float dist = dir.magnitude;
                if (Physics.Raycast(targetPosition, dir.normalized, out hitInfo, dist, cullingLayer))
                {
                    desiredPosition = targetPosition + dir.normalized * (hitInfo.distance - collisionOffset);
                }
            }

            // Установка позиции и вращения камеры
            transform.position = desiredPosition;
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position + currentState.rotationOffSet);
        }

        protected override void CameraFixed()
        {
            if (useSmooth)
            {
                currentState.Slerp(lerpState, smoothBetweenState);
            }
            else
            {
                currentState.CopyState(lerpState);
            }

            targetPosition = currentTarget.position + currentTarget.up * (offSetPlayerPivot + currentState.height);
            var pos = isValidFixedPoint ? currentState.lookPoints[indexLookPoint].positionPoint : transform.position;
            transform.position = pos;
            targetLookAt.position = targetPosition;

            if (isValidFixedPoint && currentState.lookPoints[indexLookPoint].freeRotation)
            {
                transform.rotation = Quaternion.Euler(currentState.lookPoints[indexLookPoint].eulerAngle);
            }
            else if (isValidFixedPoint)
            {
                transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
            }
            targetCamera.fieldOfView = currentState.fov;
        }
    }
}