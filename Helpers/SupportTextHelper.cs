
using System;
using System.Collections.Generic;
using System.Text;

namespace EXOApp.Helpers
{
    /// <summary>
    /// Generates Text for support page
    /// </summary>
    internal static class SupportTextHelper
    {
        /// <summary>
        /// Select the correct category, and assigns the support text to display
        /// </summary>
        /// <param name="parameter">The category to use</param>
        /// <param name="item">Specifies which support text to use in later functions</param>
        /// <param name="title">The titles of the support query</param>
        /// <param name="description">The information given to customers about the support query</param>
        public static void GetSupportInformation(string parameter, int item, out string title, out string description)
        {
            title = "";
            description = "";

            switch(parameter)
            {
                case "Control PC":
                    {
                        controlText(item, ref title, ref description);
                        break;
                    }
                case "Door Mechanism":
                    {
                        doorText(item, ref title, ref description); 
                        break;
                    }
                case "Pod":
                    {
                        podText(item, ref title, ref description);
                        break;
                    }
                case "Pump":
                    {
                        pumpText(item, ref title, ref description);
                        break;
                    }
                case "Reservoir Tanks":
                    {
                        reservoirText(item, ref title, ref description);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


        }

        /// <summary>
        /// Support Text relating to the control PC
        /// </summary>
        private static void controlText(int parameter, ref string title, ref string description)
        {
            switch(parameter)
            {
                case 0:
                    {
                        title = "I can't log into my Control PC";
                        description = "Error 1000: The Username/Password you are using are incorrect. Different logins can be found in your software manual. Alternatively, you can login using “guest” for the username and “guest” for the password. Please note that both username and password are case sensitive." +
                            "\nError 1001: The control PC is unable to find any EXO systems online. Please ensure that all EXO systems are online, and that the control PC and EXO systems are all connected to your EXO router. Also check that your control PC is not connected to internet using wi-fi." +
                            "\nError 1003: The control PC is disconnected from any network. Ensure that the ethernet cable supplied with your control PC is plugged into your EXO router and your control PC.";
                        break;
                    }
                case 1:
                    {
                        title = "My Control PC touchscreen isn't working";
                        description = "The monitor is connected to the Control PC using a USB and HDMI cable. Ensure that the USB cable is plugged into both the Control PC and the monitor. If the problem still persists, contact support@wtec.ltd";
                        break;
                    }
                case 2:
                    {
                        title = "My EXO Pod is not responding (Error 1011)";
                        description = "The EXO Pod Software is not running on your Pod. Try restarting the Pod from this App. If this doesn’t fix the error, try restarting the Pod at the breakers. If the error persists, contact our support team.";
                        break;
                    }
                case 3:
                    {
                        title = "My EXO Pod is offline (Error 1020)";
                        description = "The EXO pod has been disconnected from control PC. This could be the result of a network communication error or loss of power. First check that the control PC and EXO Pod are both connected to the EXO router. If the cables all look correct, inspect breakers. Try turning them off, wait a minute, and then turn them back on. Do not attempt this step multiple times. If this doesn’t fix the issue, contact support@wtec.ltd.";
                        break;
                    }
                case 4:
                    {
                        title = "My EXO Pod is reporting extremely high temperatures or shows Error 3030";
                        description = "There is either an internal communication error with Pod, or one of the PT100 transmitters is exhibiting errors. If the Pod no longer responds to commands when changing modes (i.e., the pump doesn’t run), try restarting the Pod using the app. If the Pod is still responding to commands, it is likely the PT100 has become defective.\nContact support@wtec.ltd for a replacement. In the meantime, if the error only occurs during floats, you can still use your Pod as normal but heated float mode will not function correctly. If the error occurs during standby, this will need to be rectified before floats can be resumed.";
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }

        /// <summary>
        ///  The support text relating the door mechanism
        /// </summary>
        private static void doorText(int parameter, ref string title, ref string description)
        {
            switch (parameter)
            {
                case 0:
                    {
                        title = "There is a squeaking noise when I operate the Lid";
                        description = "This could be due to a build-up of salt or a lack of lubrication on the mechanism. Please ensure the Door Mechanism at the rear of the pod is completely free of any salt. For safety, Isolate the door when behind the EXO Pod. Refer to our Door Mechanism Maintenance Guide for the correct lubrication locations.";
                        break;
                    }
                case 1:
                    {
                        title = "There is a Loud, Rapid Snapping Noise when door is in motion";
                        description = "This means that the Door Mechanism 5M drive belt has become un-tensioned. Please do not attempt to continue running the Lid in the event of this problem. The belts need to be re-tensioned; this can be found in our Door Mechanism Maintenance Guide.";
                        break;
                    }
                case 2:
                    {
                        title = "The Lid is not closing at the end of its motion";
                        description = "This is a result of a sensor stopping the door too early. Before attempting to fix this issue, please ensure that you are attempting to run the door from a fully open position. This is important for the Motor operation and allows the door to run to the end of its travel. Please refer to our Door Mechanism Maintenance Guide to resolve this.";
                        break;
                    }
                case 3:
                    {
                        title = "The Lid is off centre during and at the end of motion and doesn’t close correctly";
                        description = "This is due to one side of the Door Mechanism moving further forward than another, we refer to this as ‘crabbing’. Please refer to our Door Mechanism Maintenance Guide to resolve this.";
                        break;
                    }
                case 4:
                    {
                        title = "There is a large gap at the front of the lid when in a closed position";
                        description = "This means that the tilt of the Lid needs to be adjusted accordingly in order to reduce the gap. Please refer to our Door Mechanism Maintenance Guide to resolve this.";
                        break;
                    }
                case 5:
                    {
                        title = "The door operates to a close position but opens itself back up again automatically";
                        description = "This means that the Lid is falling off of the top Quadrant Sensor before reaching the end of its travel, then bouncing back onto it which then tells the Lid it needs to open. This is either due to the Gas Struts pressure is incorrect, the Position of the Gas Strut or the position of the quadrant sensor. Please refer to our Door Mechanism Maintenance Guide to resolve this or contact us at support@wtec.ltd for assistance.";
                        break;
                    }
                case 6:
                    {
                        title = "The Lid does not operate";
                        description = "Are the lights to the EXO Pod on (white on the inside base LED’s & Blue around the Halo)? If yes, this means there is the power to the EXO Pod but there may be a signal error." +
                            "\nIf no, please check the EXO Pod MCB at the wall to ensure the system has power. If there is a potential signal error, use the Door Mechanism Maintenance Guide to diagnose the error." +
                            "\nIf the Door Controller Unit is not receiving the signal from the Lid buttons, please report this to our support team.";
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }



        /// <summary>
        /// The support text relating to the Pod
        /// </summary>
        private static void podText(int parameter, ref string title, ref string description)
        {
            switch (parameter)
            {
                case 0:
                    {
                        title = "There is Hair Dye on the inside of my EXO Pod";
                        description = "Empty the EXO Pod Fully followed by closing the rear Ball Valves to isolate the pod. Spray affected area with diluted bleach and let sit for 1 hour. Spray the affected area again, then rinse off with water and drain down the EXO Pod via the rear Drain Down Tap. DO NOT MIX with EXO Pod Solution. Re-Open Ball valves at the rear of the pod and resume use of the tank.";
                        break;
                    }
                case 1:
                    {
                        title = "I tried to add Salt but my EXO Pod isn’t emptying";
                        description = "If the salt has been built in the return pipes back to the Pump, we advise running a Skimmer Mode which will circulate the solution between the Pump and the EXO Pod. This should clear the blockage. If not, please contact our support team.";
                        break;
                    }
                case 2:
                    {
                        title = "The Light / Attendant Call Button inside my EXO Pod Isn’t working";
                        description = "When operating the Light / Attendant Call Button you should hear a faint click at the rear of the pod. This ensures that the software is registering the button input. If there is no click there is an air leak from the button to the input device. Remove the black airline, inspect for damage or debris, then replace back into the fittings of the Button and Electrical Box." +
                            "Please Note: This is completely safe and uses air pressure to register the input, there is no liquid or electrics involved in its operation making it completely safe to inspect. If the button does not operate after inspection, there may be damage to the button itself and will need to be replaced.";
                        break;
                    }
                case 3:
                    {
                        title = "During a float there was no light or sound";
                        description = "Ensure the correct criteria were selected before starting the float. Please note that lighting and music criteria cannot be changed during the float. Use the ‘Demo Second’ float option to test the lighting and speakers. If this problem is not resolved from a test float, Reboot the system. If the fault is still present after a reboot, contact our support team.";
                        break;
                    }
                case 4:
                    {
                        title = "My skimmer mode doesn’t operate as expected, what do I do?";
                        description = "The Skimmer mode on the EXO Pod is calibrated at installation once the system is at all optimum levels of Solution Height (in the EXO Pod) and correct Specific Gravity (SG) (salt content). First, ensure that all these levels are correct as per the EXO Pod Operators Manual. If your levels are correct but your skimmer mode still isn’t operating correctly, the times need to be adjusted accordingly. This includes a larger fill time (and equally larger empty time – top empty the tank at the end of the skimmer cycle).";
                        break;
                    }
                case 5:
                    {
                        title = "My EXO Pod is in a standby mode but isn’t heating";
                        description = "The Heater Unit located in the pump has a thermal cut-out built into it. This may have tripped and requires a reset. On the heater unit is a button that must be pressed to reset. Also, another thing to check is the EXO Pod MCB located on the wall. Are all switches still in the on position? If the Heater Relay switched is off, turn it back on but only when the solution is flowing in a standby mode and the system is running. If the switch turns back off immediately, DO NOT ATTEMPT TO TURN BACK. Report this issue to our support team.";
                        break;
                    }
                case 6:
                    {
                        title = "My EXO Pod isn’t filling or emptying";
                        description = "Is the Control software responsive? – If yes, is the pump initialising? If no, please go to the Control PC area of the Troubleshooting. If the pump is initialising, is the colour on the Pod Halo changing to Green or Red? If yes, then the pump could be struggling to prime, which means there is excess air in the system that needs to be bled from the Filter Housings (shown in the operator’s manual). Turning on Pre-Float Warmup may help if this issue only occurs during emptying. If the pump is not initialising, the system may be unresponsive and require a reboot.";

                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }

        
        /// <summary>
        /// The support text relating to the pump
        /// </summary>
        private static void pumpText(int parameter, ref string title, ref string description)
        {
            switch (parameter)
            {
                case 0:
                    {
                        title = "My Pump isn’t running";
                        description = "It is normal for the pump to turn on and off during standby. If the Solution is at temperature, then the system will stop running and leave the solution in the Reservoir Tanks. As soon as the temperature drops below the tolerance set, the pump will turn back on to circulate and heat the solution. If the system isn’t running at all, is there power? Check the EXO Pod MCB and only attempt to turn the system back on once. If the Breaker turns off immediately after attempting to turn them on, DO NOT try to turn them back on again. Report the issue to our support team.";
                        break;
                    }
                case 1:
                    {
                        title = "There’s a loud noise coming from the pump";
                        description = "The pump can make a large amount of noise when trying to prime if there is excessive air in the system. This air needs to be bled from the Filter Housings (shown in the Maintenance Manual) debris has made it into the pump, we advise running three consecutive Demo-second floats to try and pass the item." +
                            "\nIf the debris is not passed after the first two test floats, remove the blue foam filter blocks and try again the problem persists, please contact our support team.";
                        break;
                    }
                case 2:
                    {
                        title = "How do I know if my UV Unit is working?";
                        description = "The UV Sterilisation unit located in the pump unit turns on at the same time the Heater Unit does – so when there is adequate flow for over 30 seconds. Either end of the UV Unit has Blue Caps that slightly illuminate when the UV is in operation." +
                            "\nYou can test whether or not the unit is working by turning off the lights in the room where the Pump is located and seeing the Blue Cap on top of the UV Unit slightly illuminate. If your UV Unit does not illuminate, you must first check the end caps for any Salt Debris. Then check the Connection between the UV Unit and the Control Panel (located in the Pump). If after completing these steps your UV Unit does not turn on. Please contact our support team.";
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }
        /// <summary>
        /// The support text relating to the reservoir tank
        /// </summary>
        private static void reservoirText(int parameter, ref string title, ref string description)
        {
            switch (parameter)
            {
                case 0:
                    {
                        title = "My Reservoir Tanks have over-filled, what do I do?";
                        description = "First of all, address the salt debris! The best way to tackle this is a hot cloth and Wet Vac. If the reservoir tank is overfilled – past the max fill line – – when the system is running in “Standby” there is a differential level between the two storage tanks which means one tank can overflow and then cause a leak." +
                            "\nIf the system is overfilled it’s possible to reduce the level by attaching a hose to the drain down tap at the rear of the EXO Pod or onto the reservoir tank. And lowering the level to the correct amount. It is best to put the system into STOP during this process." +
                            "\nPlease note: if the system is overfilled it’s important to make sure that the reservoir box (small box on top of the reservoir tanks). Has not got wet / filled with water. It would be worth taking the lid off and checking it is dry and clean as the salt in the solution can corrode the electronics inside the box quite quickly causing further issues.";
                        break;
                    }
                default:
                    {
                        break;
                    }

            }
        }






    }
}
