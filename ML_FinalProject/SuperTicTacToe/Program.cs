// <copyright file="Program.cs" company="Adam nassar &amp; Nathan Balcarcel">
// Copyright (c) Adam nassar &amp; Nathan Balcarcel. All rights reserved.
// </copyright>

namespace SuperTicTacToe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class that runs the program.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Board());
        }
    }
}
