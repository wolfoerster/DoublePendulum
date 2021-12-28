//******************************************************************************************
// Copyright © 2016 - 2022 Wolfgang Foerster (wolfoerster@gmx.de)
//
// This file is part of the DoublePendulum project which can be found on github.com
//
// DoublePendulum is free software: you can redistribute it and/or modify it under the terms 
// of the GNU General Public License as published by the Free Software Foundation, 
// either version 3 of the License, or (at your option) any later version.
// 
// DoublePendulum is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//******************************************************************************************

namespace DoublePendulum
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /*****
         * Image for Readme.md posted at 'https://postimages.org/'
         * Link:
         * Direct link:
         * Markdown:
         * Alt Markdown:
         * Thumbnail for forums:
         * Thumbnail for website:
         * Hotlink for forums:
         * Hotlink for website:
         * Removal link:
         *****/

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string theme = "PresentationFramework.Aero;V3.0.0.0;31bf3856ad364e35;component\\themes/aero.normalcolor.xaml";
            //string theme = "/PresentationFramework.Classic;v3.0.0.0;31bf3856ad364e35;Component/themes/classic.xaml";
            //string theme = "/PresentationFramework.Royale;v3.0.0.0;31bf3856ad364e35;Component/themes/royale.normalcolor.xaml";
            //string theme = "/PresentationFramework.Luna;v3.0.0.0;31bf3856ad364e35;Component/themes/luna.normalcolor.xaml";
            //string theme = "/PresentationFramework.Luna;v3.0.0.0;31bf3856ad364e35;Component/themes/luna.homestead.xaml";
            //string theme = "/PresentationFramework.Luna;v3.0.0.0;31bf3856ad364e35;Component/themes/luna.metallic.xaml";
            Uri uri = new Uri(theme, UriKind.Relative);
            Resources.MergedDictionaries.Add(Application.LoadComponent(uri) as ResourceDictionary);
        }

        public static List<Pendulum> Pendulums = new List<Pendulum>();

        public static Pendulum SelectedPendulum = new Pendulum();
    }
}
