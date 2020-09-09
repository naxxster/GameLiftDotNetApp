#!/usr/bin/env bash

# install mono
sudo amazon-linux-extras enable mono && sudo yum -y install mono-complete 
sudo yum -y update

# make symbolic link to mono for starting the process with Amazon GameLift Fleet
ln -s /usr/bin/mono /game/local/mono
