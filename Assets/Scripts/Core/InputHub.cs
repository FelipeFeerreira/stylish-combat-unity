using UnityEngine;
using UnityEngine.InputSystem;

namespace HnS.Core
{
    public struct FrameInput
    {
        public Vector2 move;
        public Vector2 look;
        public bool attack;
        public bool heavy;
        public bool jump;
        public bool dodge;
        public bool lockToggle;
        public bool shoot;
    }

    public static class InputHub
    {
        static bool failed;

        public static void ResetFailure() { failed = false; }

        public static FrameInput GetFrame()
        {
            FrameInput f = default;
            if (failed) return f;

            try
            {
                var k = Keyboard.current;
                var m = Mouse.current;
                var g = Gamepad.current;

                Vector2 gv = Vector2.zero;
                if (g != null)
                {
                    gv = g.leftStick.ReadValue();
                    if (gv.sqrMagnitude > 0.04f)
                    {
                        f.move = gv.sqrMagnitude > 1f ? gv.normalized : gv;
                    }
                }

                if (k != null)
                {
                    Vector2 mv = Vector2.zero;
                    if (k.wKey.isPressed) mv.y += 1f;
                    if (k.sKey.isPressed) mv.y -= 1f;
                    if (k.aKey.isPressed) mv.x -= 1f;
                    if (k.dKey.isPressed) mv.x += 1f;
                    if (mv.sqrMagnitude > 1f) mv.Normalize();
                    if (mv.sqrMagnitude > f.move.sqrMagnitude || gv.sqrMagnitude <= 0.04f) f.move = mv;

                    if (m != null)
                    {
                        f.look = m.delta.ReadValue();
                        f.attack = m.leftButton.wasPressedThisFrame;
                        f.heavy = m.rightButton.wasPressedThisFrame;
                    }

                    f.jump = k.spaceKey.wasPressedThisFrame;
                    f.dodge = k.leftShiftKey.wasPressedThisFrame;
                    f.lockToggle = k.tabKey.wasPressedThisFrame || k.qKey.wasPressedThisFrame;
                    f.shoot = k.eKey.wasPressedThisFrame;
                }

                if (g != null)
                {
                    Vector2 ls = g.leftStick.ReadValue();
                    if (ls.sqrMagnitude > 0.04f)
                    {
                        f.move = ls.sqrMagnitude > 1f ? ls.normalized : ls;
                    }

                    Vector2 r = g.rightStick.ReadValue();
                    if (r.sqrMagnitude >= 0.01f) f.look = r;

                    f.attack |= g.buttonSouth.wasPressedThisFrame;
                    f.heavy |= g.buttonNorth.wasPressedThisFrame;
                    f.jump |= g.buttonWest.wasPressedThisFrame;
                    f.dodge |= g.buttonEast.wasPressedThisFrame;
                    f.lockToggle |= g.rightStickButton.wasPressedThisFrame;
                    f.shoot |= g.leftShoulder.wasPressedThisFrame;
                }
            }
            catch (System.InvalidOperationException)
            {
                failed = true;
                return default;
            }

            return f;
        }
    }
}