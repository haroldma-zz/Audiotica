Audiotica
=========
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/zumicts/Audiotica?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License APACHE](https://img.shields.io/badge/license-APACHE-642C90.svg?style=flat-square)](https://raw.githubusercontent.com/zumicts/audiotica/master/LICENSE)
[![Contact](https://img.shields.io/badge/contact-@Zumicts-642C90.svg?style=flat-square)](https://twitter.com/zumicts)
[![Gratipay](https://img.shields.io/gratipay/zumicts.svg?style=flat-square)](https://gratipay.com/zumicts)

The All-in-One music app, powered by Last.FM, Spotify and Deezer.

## Requirements

Most dependencies are obtained from Nuget and automatically restored on build.  The only external dependency you'll have install is the [Windows Ad Mediator](https://visualstudiogallery.msdn.microsoft.com/401703a0-263e-4949-8f0f-738305d6ef4b) SDK and [SQLite for Universal App Platform](https://visualstudiogallery.msdn.microsoft.com/4913e7d5-96c9-4dde-a1a1-69820d615936) SDK.

## Building

Make sure you have the necessary tools for building [Windows Universal Apps](https://dev.windows.com/en-us/develop/building-universal-Windows-apps).

Simply clone the repo

    git clone https://github.com/zumicts/audiotica

Open the solution file `Audiotica.sln` in Visual Studio.  Make sure the selected platform is either `x86`, for emulator, or `ARM` for device.  Then right-click and click Build on the solution.  Nuget should download all missing packages, if not open the package manager and click `Restore Missing Packages`.

You should now have successfully build the Audiotica app.  Unless you don't have one of the required SDKs installed or are in any other branch besides `master`, everything should be working.
