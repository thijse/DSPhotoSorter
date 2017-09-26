DSPhotoSorter - Photo sorter and deduplicator for Synology DS Photo
====================

[![License](https://img.shields.io/badge/license-MIT%20License-blue.svg)](http://doge.mit-license.org)

*A command line tool for sorting photos from the Synology DSPhoto auto-upload tool .*

DSPhotoSorter is a commandline tool that processes photostreams on a Synology NAS. Synology supplies a Iphone & Android app that auto-uploads photos from the camera roll of the phone to a selected Synology NAS without much additional intelligence.

This tool helps organize photos (e.g. for retaining only relevant family photos). It does this by taking the images (& movies) frome these photo streams, sees if they are unique and if so copies the file into a <ROOT>\Sorted\<YEAR>\<MONTH> folder structure. This helps, for example, with images that are shared through WhatsApp group and end up in everybodies photo roll.  Additionally, it notes which photos have been copied, so that if these images are then deleted from the <ROOT>\Sorted folders they will not come back.

## Features

* Fast deduplication of photos
* Filename-based Year & month sorting 
* Copy-once 
* MIT License

## Tested  

* C# .NET
* MONO, .NET core are to be tested. This would be useful so that the tool could run on the NAS or a Raspberry Pi

## Downloading

This package can only be downloaded from GitHub. 

- By directly loading fetching the Archive from GitHub: 
 1. Go to [https://github.com/thijse/Arduino-Log](https://github.com/thijse/Arduino-Log)
 2. Click the DOWNLOAD ZIP button in the panel on the
 3. Optionally rename the uncompressed folder **DSPhotoSorter-master** to **DSPhotoSorter**.

- By downloading a release

## Configuration

When run the first time, the application will create a configuration.json file which you will need to update and place in the same folder as the DsPhotoSorter

```c++
    Serial.begin(9600);
    
    // Initialize with log level and log output. 
    Log.begin   (LOG_LEVEL_VERBOSE, &Serial);
    
    // Start logging text and formatted values
    Log.error   (  "Log as Error   with binary values             : %b, %B"CR  , 23  , 345808);
    Log.warning (F("Log as Warning with integer values from Flash : %d, %d"CR) , 34  , 799870);
```


## Copyright

DSPhotoSorter is provided Copyright © 2017 under MIT License.

