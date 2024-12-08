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
         * Link: https://postimg.cc/HjBkY7VY
         * Direct link: https://i.postimg.cc/7YyGN342/Double-Pendulum.jpg
         * Markdown: [Double-Pendulum.jpg](https://postimg.cc/HjBkY7VY)
         * Alt Markdown: [![Double-Pendulum.jpg](https://i.postimg.cc/7YyGN342/Double-Pendulum.jpg)](https://postimg.cc/HjBkY7VY)
         * Thumbnail for forums: [url=https://postimg.cc/HjBkY7VY][img]https://i.postimg.cc/HjBkY7VY/Double-Pendulum.jpg[/img][/url]
         * Thumbnail for website: <a href='https://postimg.cc/HjBkY7VY' target='_blank'><img src='https://i.postimg.cc/HjBkY7VY/Double-Pendulum.jpg' border='0' alt='Double-Pendulum'/></a>
         * Hotlink for forums: [url=https://postimages.org/][img]https://i.postimg.cc/7YyGN342/Double-Pendulum.jpg[/img][/url]
         * Hotlink for website: <a href='https://postimages.org/' target='_blank'><img src='https://i.postimg.cc/7YyGN342/Double-Pendulum.jpg' border='0' alt='Double-Pendulum'/></a>
         * Removal link: https://postimg.cc/delete/WKsFY9GY/a68bc2c6
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

        public static List<Pendulum> VisiblePendulums
        {
            get 
            {
                var soloed = new List<Pendulum>();
                var unmuted = new List<Pendulum>();

                foreach (var pendulum in App.Pendulums)
                {
                    if (pendulum.IsSoloed)
                        soloed.Add(pendulum);

                    else if (!pendulum.IsMuted)
                        unmuted.Add(pendulum);
                }

                return soloed.Count > 0 ? soloed : unmuted;
            }
        }
    }
}
