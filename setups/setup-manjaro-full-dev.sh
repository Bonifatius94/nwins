# =======================================================
#         M A N J A R O    L I N U X    S E T U P
# =======================================================

# update OS packages
sudo pacman -Syu --noconfirm

# install dotnet sdk
wget https://dot.net/v1/dotnet-install.sh
chmod 755 dotnet-install.sh
sudo ./dotnet-install.sh --install-dir /usr/share/dotnet -channel Current -version latest

# install mono and msbuild tools
sudo pacman -S mono mono-msbuild --noconfirm

# download Godot Engine with Mono / C# support (see: https://godotengine.org/download/linux)
cd ~ && \
wget https://downloads.tuxfamily.org/godotengine/3.2.3/mono/Godot_v3.2.3-stable_mono_x11_64.zip && \
unzip Godot_v3.2.3-stable_mono_x11_64.zip && \
rm -rf Godot_v3.2.3-stable_mono_x11_64.zip && \
mv 'Godot_v3.2.3-stable_mono_x11_64' godot-mono
cd ~/godot-mono && \
mv 'Godot_v3.2.3-stable_mono_x11.64' godot-mono && \
echo 'export PATH="~/godot-mono:$PATH"' >> ~/.bashrc

# install docker
sudo pacman -S docker --noconfirm
sudo systemctl start docker.service
sudo systemctl enable docker.service
sudo usermod -aG docker $USER
reboot