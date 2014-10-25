Audiotica
=========
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/zumicts/Audiotica?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License MIT](https://img.shields.io/badge/license-MIT-642C90.svg?style=flat-square)](https://raw.githubusercontent.com/zumicts/audiotica/master/LICENSE)
[![Contact](https://img.shields.io/badge/contact-@Zumicts-642C90.svg?style=flat-square)](https://twitter.com/zumicts)
[![Gratipay](https://img.shields.io/gratipay/zumicts.svg?style=flat-square)](https://gratipay.com/zumicts)

Music player powered by LastFM, that uses Vk, YouTube, Soundcloud and other sites to download music.

## Requirements

The music library is powered by SQLite.  You'll need both [WinRT 8.1](http://visualstudiogallery.msdn.microsoft.com/1d04f82f-2fe9-4727-a2f9-a2db127ddc9a) and [WinPRT 8.1](http://visualstudiogallery.msdn.microsoft.com/5d97faf6-39e3-4048-a0bc-adde2af75d1b) installed in your computer.

## Building

Make sure you have the necessary tools for building [Windows Universal Apps](https://dev.windows.com/en-us/develop/building-universal-Windows-apps).

Simply clone the repo

    git clone https://github.com/zumicts/audiotica

Open the solution file `Audiotica.sln` in Visual Studio.  Make sure the selected platform is either `x86`, for emulator, or `ARM` for device.  Then right-click and click Build on the `Audiotica.WindowsPhone` project in the `Apps` folder.  Nuget should download all missing packages, if not open the package manager and click `Restore Missing Packages`.

You should now have successfully build the Audiotica app for Windows Phone.  Unless you don't have the SQLite SDK installed or are in any other branch besides `master`, everything should be working.

**NOTE: As of this writing the Windows project will not build.**  This project has not be worked on yet.
