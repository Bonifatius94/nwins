# ============================================
#        UBUNTU 20.04 MACHINE SETUP
# ============================================

# info: run this script with 'chmod 755 setup-godot-mono.sh && sudo ./setup-godot-mono.sh'

# install Dotnet Core SDK 5.0 (see https://docs.microsoft.com/de-de/dotnet/core/install/linux-ubuntu#2004-)
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get update; \
apt-get install -y apt-transport-https && \
apt-get update && \
apt-get install -y dotnet-sdk-5.0

# install Mono SDK 6.12.0 (see https://www.mono-project.com/download/stable/#download-lin-ubuntu)
apt install -y gnupg ca-certificates && \
apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
apt update && \
apt install -y mono-complete

# download Godot Engine with Mono / C# support (see: https://godotengine.org/download/linux)
cd ~ && \
wget https://downloads.tuxfamily.org/godotengine/3.2.3/mono/Godot_v3.2.3-stable_mono_x11_64.zip && \
unzip Godot_v3.2.3-stable_mono_x11_64.zip && \
rm -rf Godot_v3.2.3-stable_mono_x11_64.zip && \
mv 'Godot_v3.2.3-stable_mono_x11_64' godot-mono

# prepare / register Godot Engine CLI tools
cd ~/godot-mono && \
mv 'Godot_v3.2.3-stable_mono_x11.64' godot-mono && \
echo 'export PATH="~/godot-mono:$PATH"' >> ~/.bashrc

# install docker and reboot
sudo apt-get update && sudo apt-get install -y docker.io docker-compose
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker $USER
reboot

# ============================================
#         Marco Tröster - 2020-12-13
# ============================================
