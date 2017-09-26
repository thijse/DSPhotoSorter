DSPhotoSorter - Photo sorter and deduplicator for Synology DS Photo
====================

[![License](https://img.shields.io/badge/license-MIT%20License-blue.svg)](http://doge.mit-license.org)

*A command line tool for sorting photos from the Synology DSPhoto auto-upload tool .*

DSPhotoSorter is a commandline tool that processes photostreams on a Synology NAS. Synology supplies a Iphone & Android app that auto-uploads photos from the camera roll of the phone to a selected Synology NAS without much additional intelligence.

This tool helps organize photos (e.g. for retaining only relevant family photos). It does this by taking the images (& movies) frome these photo streams, sees if they are unique and if so copies the file into a <DestinationPath><YEAR>\<MONTH> folder structure. This helps, for example, with images that are shared through WhatsApp group and end up in everybodies photo roll.  Additionally, it notes which photos have been copied, so that if these images are then deleted from the <ROOT>\Sorted folders they will not come back.

## Features

* Fast deduplication of photos
* Filename-based Year & month sorting 
* Copy only, no files are deleted
* Copy images only once 
* MIT License

## Tested  

* C# .NET
* MONO, .NET core are to be tested. This would be useful so that the tool could run on the NAS or a Raspberry Pi

## Downloading

This Application is not available through Nuget can only be downloaded from GitHub. 

- By directly loading fetching the Archive from GitHub: 
  1. Go to [https://github.com/thijse/DSPhotoSorter](https://github.com/thijse/DSPhotoSorter)
  2. Click the DOWNLOAD ZIP button in the panel on the
  3. Optionally rename the uncompressed folder **DSPhotoSorter-master** to **DSPhotoSorter**.

- By downloading a release
  1. Go to releases
  2. Click the 'Source code' button


## Configuration

When run the first time, the application will create a configuration.json file which you will need to update and place in the same folder as the DsPhotoSorter

```json
{
  "DestinationPath":   "\\\\NAS\\photo\\Sorted\\",
  "DuplicatesPath":    "\\\\NAS\\photo\\Duplicates\\",
  "ConfigurationPath": "\\\\NAS\\photo\\Configuration\\",
  "Sources": [
    {
      "Path":          "\\\\NAS\\photo\\Camera Roll Name1",
      "Postfix":       "_name1"
    },
    {
      "Path":          "\\\\NAS\\photo\\Camera Roll Name2",
      "Postfix":       "_name2"
    }
  ],
  "MapRootFrom":        "\\\\192.168.1.1",
  "MapRootTo":          "\\\\NAS"
}
```

* DestinationPath   - The root folder where unique images will be copied to and sorted
* DuplicatesPath    - The folder where duplicate images will be copied to. This folder could be cleared occasionally
* ConfigurationPath - The folder where a list of previously images are kept (processedFiles.txt)
* Sources > Path    - The folder of each photo roll
* Sources > Postfix - A postfix to be attached to the images. Can be used to indicate the source. 

If the tool is used with different URIs to access the images, the paths in the processedFiles.txt would differ, and files could be copied again. This mapping is used to give them a consistent path in processedFiles.txt.

* MapRootFrom       - the root path as the images are accessed by this instance of the sorter
* MapRootTo         - the root path of how the images are stored 

## Copyright

DSPhotoSorter is provided Copyright © 2017 under MIT License.

