//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: RoboticsTutorial4Form.cs $ $Revision: 9 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Dss.ServiceModel.Dssp;

namespace Microsoft.Robotics.Services.RoboticsTutorial4
{
    public partial class RoboticsTutorial4Form : Form
    {
        RoboticsTutorial4Operations _mainPort;

        Dictionary<String, bool> values = new Dictionary<string, bool>();

        public RoboticsTutorial4Form(RoboticsTutorial4Operations mainPort)
        {
            _mainPort = mainPort;

            InitializeComponent();

            values.Add("bumper1", false);
            values.Add("bumper2", false);

            UpdateBumperText();
        }

        private void UpdateBumperText()
        {
            lblBumperStatus.Text = String.Format("Bumper 1: {0}\nBumper 2: {1}", values["bumper1"], values["bumper2"]); ;
        }

        #region CODECLIP 03-2
        protected override void OnClosed(EventArgs e)
        {
            _mainPort.Post(new DsspDefaultDrop(DropRequestType.Instance));

            base.OnClosed(e);
        }
        #endregion

        #region CODECLIP 03-1
        private void btnStop_Click(object sender, EventArgs e)
        {
            _mainPort.Post(new Stop());
        }
        #endregion

        private void btnForward_Click(object sender, EventArgs e)
        {
            _mainPort.Post(new Forward());
        }

        private void btnBackward_Click(object sender, EventArgs e)
        {
            _mainPort.Post(new Backward());
        }

        private void tnTurnLeft_Click(object sender, EventArgs e)
        {
            _mainPort.Post(new TurnLeft());
        }

        private void btnTurnRight_Click(object sender, EventArgs e)
        {
            _mainPort.Post(new TurnRight());
        }

        internal void OnBumperChange(string p1, bool p2)
        {
            values[p1] = p2;
            UpdateBumperText();
        }
    }
}