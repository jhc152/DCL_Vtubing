#!/bin/bash

# Check if Homebrew is installed
# if test ! $(which brew); then
#   echo "Installing Homebrew..."
#   /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
# fi

if test ! $(which brew); then
  echo "Installing Homebrew..."
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
fi

# Install dependencies
brew install v4l2loopback

# Load the v4l2loopback kernel module
//sudo kextload /usr/local/opt/v4l2loopback/v4l2loopback.kext
sudo -S kextload /usr/local/opt/v4l2loopback/v4l2loopback.kext

# Register the virtual camera device
sudo modprobe v4l2loopback \
  video_nr=10 \
  card_label="DCL Vtubing Camera" \
  exclusive_caps=1 \
  max_buffers=2

echo "Virtual camera installed successfully."
