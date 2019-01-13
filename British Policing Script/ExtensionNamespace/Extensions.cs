using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using Rage.Native;
using System.Drawing;


namespace British_Policing_Script
{
    static class Extensions
    {      
        public static void DisEnableControls(bool enable)
        {
            foreach (var con in Enum.GetValues(typeof(GameControl)))
            {
                if (enable)
                {
                    NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)con);
                    NativeFunction.Natives.ENABLE_CONTROL_ACTION(1, (int)con);
                    NativeFunction.Natives.ENABLE_CONTROL_ACTION(2, (int)con);
                }
                else
                {
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, (int)con);
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(1, (int)con);
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(2, (int)con);
                }
            }
            //Controls we want
            // -Frontend
            // -Mouse
            // -Walk/Move
            // -

            if (enable) return;
            var list = new List<GameControl>
            {
                GameControl.FrontendAccept,
                GameControl.FrontendAxisX,
                GameControl.FrontendAxisY,
                GameControl.FrontendDown,
                GameControl.FrontendUp,
                GameControl.FrontendLeft,
                GameControl.FrontendRight,
                GameControl.FrontendCancel,
                GameControl.FrontendSelect,
                GameControl.CursorScrollDown,
                GameControl.CursorScrollUp,
                GameControl.CursorX,
                GameControl.CursorY,
                GameControl.MoveUpDown,
                GameControl.MoveLeftRight,
                GameControl.Sprint,
                GameControl.Jump,
                GameControl.Enter,
                GameControl.VehicleExit,
                GameControl.VehicleAccelerate,
                GameControl.VehicleBrake,
                GameControl.VehicleMoveLeftRight,
                GameControl.VehicleFlyYawLeft,
                GameControl.ScriptedFlyLeftRight,
                GameControl.ScriptedFlyUpDown,
                GameControl.VehicleFlyYawRight,
                GameControl.VehicleHandbrake,




            };

            if (Game.IsControllerConnected)
            {
                list.AddRange(new GameControl[]
                {
                    GameControl.LookUpDown,
                    GameControl.LookLeftRight,
                    GameControl.Aim,
                    GameControl.Attack,
                });
            }

            foreach (var control in list)
            {
                NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)control);
            }
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)GameControl.LookLeftRight);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)GameControl.LookUpDown);
        }
    }
}

