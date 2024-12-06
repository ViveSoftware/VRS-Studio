# VRS Studio
Copyright 2021-2024, HTC Corporation. All rights reserved.

<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

VRS Studio is a project created by HTC VIVE to demonstrate the features of VIVE Focus Vision and other devices, as well as to show a variety of different use cases of said capabilities by using our SDK and VIU toolkit.

<p align="right">(<a href="#top">back to top</a>)</p>

### Built With

* [VIVE Wave SDK](https://developer.vive.com/resources/vive-wave/)
* [VIVE Input Utility for Unity](https://github.com/ViveSoftware/ViveInputUtility-Unity)
* [DOTween](http://dotween.demigiant.com/)

### Verified Devices

* VIVE Focus 3、VIVE XR Elite、VIVE Focus Vision

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

- Unity 2021.3.16f1 (With Android Build Support)

### Instructions

 1. Clone the repo.
  ```sh
    git clone https://github.com/ViveSoftware/VRS-Studio.git
  ```
 2. In Unity Hub, select "Add project from disk" and choose the "VRS_Studio" project folder.
 3. Ensure that the editor version used is "2021.3.16f1" and the platform is set to "Android".
 4. After the project is loaded, verify that the feature is enabled in Project Settings > XR Plug-in Management > WaveXRSettings.
     - Allow Spectator Camera
     - Allow Capture 360 Image
     - Enable Tracker
     - Enable Natural Hand
     - Enable Eye Tracking
     - Enable Eye Expression
     - Enable Lip Expression
     - Enable Body Tracking
5. To build the APK, go to the menu bar and select VRS_Studio > Build > Do Build. The APK will be located in the "builds" folder within the project directory.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- LICENSE -->
## License

See `LICENSE.pdf` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

Project Link: [https://github.com/ViveSoftware/VRS-Studio](https://github.com/ViveSoftware/VRS-Studio)

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [My take on shaders: Teleportation dissolve](https://halisavakis.com/my-take-on-shaders-teleportation-dissolve/) - Used as teleportation VFX of the robot

<p align="right">(<a href="#top">back to top</a>)</p>