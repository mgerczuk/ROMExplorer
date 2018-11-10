# ROM Explorer
Browse Android ROM files on Windows.

ROM Explorer supports the following file formats:
- *.img files (raw and sparse ext4)
- *.zip archives with img, system.new.dat or system.new.dat.br
- *.md tar archives (Samsung)
- update.app archives (Huawei)

You can browse files and extract them by dragging to Windows Explorer.

## Used 3rd Party Libraries

ROM Explorer uses source code from the following open source libraries

### DiscUtils

https://github.com/DiscUtils/DiscUtils

Some changes are made to the code to accept unresolved symbolic links.

### HuaweiUpdateLibrary

https://github.com/worstenbrood/HuaweiUpdateLibrary


## Todo

- Support modifying the ROM file. This is not easy because the DiscUtils library does not support modifying an ext4 file system.
- Support for Sony ftf files