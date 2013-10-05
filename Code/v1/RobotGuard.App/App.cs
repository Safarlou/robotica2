//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial4.cs $ $Revision: 22 $
//-----------------------------------------------------------------------
using Microsoft.Ccr.Core;
using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using xml = System.Xml;
using W3C.Soap;
using Microsoft.Robotics.Services.RoboticsTutorial4.Properties;
using System.ComponentModel;

using bumper = Microsoft.Robotics.Services.ContactSensor.Proxy;
using motor = Microsoft.Robotics.Services.Motor.Proxy;

using brick = Microsoft.Robotics.Services.Sample.Lego.Nxt.Brick;
using color = Microsoft.Robotics.Services.Sample.Lego.Nxt.ColorSensor;

using analog = Microsoft.Robotics.Services.AnalogSensor.Proxy;

using submgr = Microsoft.Dss.Services.SubscriptionManager;

using System.Windows.Forms;

namespace Microsoft.Robotics.Services.RoboticsTutorial4
{
    [DisplayName("(User) Robotics Tutorial 4 (C#): Drive-By-Wire")]
    [Description("This tutorial demonstrates how to create a service that partners with abstract, base definitions of hardware services.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483053.aspx")]
    [Contract(Contract.Identifier)]
    public class RoboticsTutorial4 : DsspServiceBase
    {
        private RoboticsTutorial4Form form = null;


        [ServiceState]
        private RoboticsTutorial4State _state = new RoboticsTutorial4State();

        [ServicePort("/RoboticsTutorial4", AllowMultipleInstances=false)]
        private RoboticsTutorial4Operations _mainPort = new RoboticsTutorial4Operations();

        [Partner("bumper1", Contract = bumper.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private bumper.ContactSensorArrayOperations _bumperPort1 = new bumper.ContactSensorArrayOperations();
        private bumper.ContactSensorArrayOperations _bumperNotify1 = new bumper.ContactSensorArrayOperations();

        [Partner("bumper2", Contract = bumper.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private bumper.ContactSensorArrayOperations _bumperPort2 = new bumper.ContactSensorArrayOperations();
        private bumper.ContactSensorArrayOperations _bumperNotify2 = new bumper.ContactSensorArrayOperations();


        const float maxspeed = 0.1f;

        [Partner("motor1", Contract = motor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private motor.MotorOperations _motorPort1 = new motor.MotorOperations();
        private motor.MotorOperations _motorNotify1 = new motor.MotorOperations();

        [Partner("motor2", Contract = motor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private motor.MotorOperations _motorPort2 = new motor.MotorOperations();
        private motor.MotorOperations _motorNotify2 = new motor.MotorOperations();

        [Partner("motor3", Contract = motor.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private motor.MotorOperations _motorPort3 = new motor.MotorOperations();
        private motor.MotorOperations _motorNotify3 = new motor.MotorOperations();

        /*
        [Partner("sonar", Contract = sonar.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private sonar.SonarOperations _sonarPort = new sonar.SonarOperations();
        private sonar.SonarOperations _sonarNotify = new sonar.SonarOperations();

        [Partner("analog", Contract = analog.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private analog.AnalogSensorOperations _analogPort = new analog.AnalogSensorOperations();
        private analog.AnalogSensorOperations _analogNotify = new analog.AnalogSensorOperations();*/

        /*
        [Partner("color", Contract = color.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExisting)]
        private color.ColorSensorOperations _colorPort = new color.ColorSensorOperations();
        private color.ColorSensorOperations _colorNotify = new color.ColorSensorOperations();
        */
        /*
        [Partner(Partners.SubscriptionManagerString, Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();*/

        public RoboticsTutorial4(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {

        }

        #region CODECLIP 02-1
        protected override void Start()
        {
            base.Start();

            WinFormsServicePort.Post(new RunForm(StartForm));

            #region CODECLIP 01-5

            // Subscribe to the bumper service, receive notifications on the bumperNotificationPort.
            _bumperPort1.Subscribe(_bumperNotify1);
            _bumperPort2.Subscribe(_bumperNotify2);
            
            //_colorPort.Subscribe(_colorNotify);
            //_analogPort.Subscribe(_analogNotify);
            //_sonarPort.Subscribe(_sonarNotify);
            

            // Start listening for updates from the bumper service.
            Activate( Arbiter.Receive<bumper.Update> (true, _bumperNotify1, BumperHandler));
            Activate( Arbiter.Receive<bumper.Update> (true, _bumperNotify2, BumperHandler));
            //Activate( Arbiter.Receive<brick.LegoSensorUpdate> (true, _colorNotify, null));

            //Activate(Arbiter.Receive<sonar.Update>(true, _sonarNotify, SonarHandler));
            #endregion
        }
        #endregion

        private void BumperHandler(bumper.Update update)
        {
            Invoke(delegate()
            {
                form.OnBumperChange(update.Body.Name, update.Body.Pressed);
            }
            );
            return;
        }

        /*
        private void AnalogHandler(analog.Update update)
        {
            return;
        }
        private void SonarHandler(sonar.Update update)
        {
            return;
        }*/

        #region CODECLIP 02-2
        private System.Windows.Forms.Form StartForm()
        {
            form = new RoboticsTutorial4Form(_mainPort);

            Invoke(delegate()
                {
                    PartnerType partner = FindPartner("bumper1");
                    Uri uri = new Uri(partner.Service);
                    form.Text = string.Format(
                        Resources.Culture,
                        Resources.Title,
                        uri.AbsolutePath
                    );
                }
            ); 
            Invoke(delegate()
            {
                PartnerType partner = FindPartner("bumper2");
                Uri uri = new Uri(partner.Service);
                form.Text = string.Format(
                    Resources.Culture,
                    Resources.Title,
                    uri.AbsolutePath
                );
            }
             );

            Invoke(delegate()
            {
                PartnerType partner = FindPartner("motor1");
                Uri uri = new Uri(partner.Service);
                form.Text = string.Format(
                    Resources.Culture,
                    Resources.Title,
                    uri.AbsolutePath
                );
            }
            );

            Invoke(delegate()
            {
                PartnerType partner = FindPartner("motor2");
                Uri uri = new Uri(partner.Service);
                form.Text = string.Format(
                    Resources.Culture,
                    Resources.Title,
                    uri.AbsolutePath
                );
            }
            );

            Invoke(delegate()
            {
                PartnerType partner = FindPartner("motor3");
                Uri uri = new Uri(partner.Service);
                form.Text = string.Format(
                    Resources.Culture,
                    Resources.Title,
                    uri.AbsolutePath
                );
            }
            );

            /*
            Invoke(delegate()
            {
                PartnerType partner = FindPartner("analog");
                Uri uri = new Uri(partner.Service);
                form.Text = string.Format(
                    Resources.Culture,
                    Resources.Title,
                    uri.AbsolutePath
                );
            }
            );

            Invoke(delegate()
            {
                PartnerType partner = FindPartner("sonar");
                Uri uri = new Uri(partner.Service);
                form.Text = string.Format(
                    Resources.Culture,
                    Resources.Title,
                    uri.AbsolutePath
                );
            }
            );
            */

            return form;
        }
        #endregion

        #region CODECLIP 02-3
        private void Invoke(System.Windows.Forms.MethodInvoker mi)
        {
            WinFormsServicePort.Post(new FormInvoke(mi));
        }
        #endregion


        /// <summary>
        /// Replace Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        private IEnumerator<ITask> SetWheelPowers(float motor2, float motor3)
        {
            motor.SetMotorPowerRequest request1 = new motor.SetMotorPowerRequest();
            request1.TargetPower = motor2;

            yield return Arbiter.Choice(
                _motorPort2.SetMotorPower(request1),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to drive forwards", fault);
                }
            );

            motor.SetMotorPowerRequest request2 = new motor.SetMotorPowerRequest();
            request2.TargetPower = motor3;
            yield return Arbiter.Choice(
                _motorPort3.SetMotorPower(request2),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to drive forwards", fault);
                }
            );
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> StopHandler(Stop stop)
        {
            /*
            drive.SetDrivePowerRequest request = new drive.SetDrivePowerRequest();
            request.LeftWheelPower = 0;
            request.RightWheelPower = 0;

            yield return Arbiter.Choice(
                _drivePort.SetDrivePower(request),
                delegate(DefaultUpdateResponseType response) { },
                delegate(Fault fault)
                {
                    LogError(null, "Unable to stop", fault);
                }
            );*/

            return SetWheelPowers(0.0f, 0.0f);
        }


        #region CODECLIP 01-3
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> ForwardHandler(Forward forward)
        {
            return SetWheelPowers(maxspeed, maxspeed);
        }
        #endregion

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> BackwardHandler(Backward backward)
        {
            return SetWheelPowers(-maxspeed, -maxspeed);
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> TurnLeftHandler(TurnLeft turnLeft)
        {
            return SetWheelPowers(-maxspeed, maxspeed);
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ITask> TurnRightHandler(TurnRight forward)
        {
            return SetWheelPowers(maxspeed, -maxspeed);
        }

        /*
        #region CODECLIP 01-6
        private void NotifyDriveUpdate(drive.Update update)
        {
            RoboticsTutorial4State state = new RoboticsTutorial4State();
            state.MotorEnabled = update.Body.IsEnabled;

            _mainPort.Post(new Replace(state));
        }
        #endregion
         */
    }
}
