PLATFORM = $*
DOWNLOAD_URL = "https://gitlab.com/api/v4/projects/giomasce%2Fdqib/jobs/artifacts/master/download?job=convert_$(PLATFORM)"
BASE_DIR = $(PLATFORM)-debian
DL_NAME = dqib_$(PLATFORM)
DIR = $(BASE_DIR)/$(DL_NAME)
DIR_EXTRA = $(BASE_DIR)/extra

RISCV64_EXTRA_BASE_URL = "https://github.com/Apress/RISC-V-Assembly-Language-Programming/raw/main/Chapter%201/QEMU"


all:
	@echo 'Please select a specific rule: {install|run|ssh}-{x86-64|riscv64}'


# `install` rules.
install-x86-64: install-image-amd64-pc
install-riscv64: install-image-riscv64-virt install-extra-riscv64-virt

install-image-%:
	mkdir -p $(BASE_DIR)
	wget $(DOWNLOAD_URL) -O $(BASE_DIR)/$(DL_NAME).zip
	unzip -d $(BASE_DIR) $(BASE_DIR)/$(DL_NAME).zip

install-extra-%:
	mkdir -p $(DIR_EXTRA)
	wget $(RISCV64_EXTRA_BASE_URL)/fw_jump.elf -O $(DIR_EXTRA)/fw_jump.elf
	wget $(RISCV64_EXTRA_BASE_URL)/uboot.elf -O $(DIR_EXTRA)/uboot.elf


# `run` rules.
run-x86-64: run-x86-64-amd64-pc
run-riscv64: run-riscv64-riscv64-virt

run-x86-64-%:
	qemu-system-x86_64 \
		-machine 'pc' \
		-cpu 'Nehalem' \
		-m 4G \
		-drive file=$(DIR)/image.qcow2 \
		-device e1000,netdev=net \
		-netdev user,id=net,hostfwd=tcp::2222-:22 \
		-kernel $(DIR)/kernel \
		-initrd $(DIR)/initrd \
		-nographic \
		-append "root=LABEL=rootfs console=ttyS0" \
		-fsdev local,id=virtfs0,path=../,security_model=mapped-xattr \
		-device virtio-9p-pci,fsdev=virtfs0,mount_tag=share

run-riscv64-%:
	qemu-system-riscv64 \
		-machine 'virt' \
		-cpu 'rv64' \
		-m 4G \
		-device virtio-blk-device,drive=hd \
		-drive file=$(DIR)/image.qcow2,if=none,id=hd \
		-device virtio-net-device,netdev=net \
		-netdev user,id=net,hostfwd=tcp::2223-:22 \
		-bios $(DIR_EXTRA)/fw_jump.elf \
		-kernel $(DIR_EXTRA)/uboot.elf \
		-object rng-random,filename=/dev/urandom,id=rng \
		-device virtio-rng-device,rng=rng \
		-nographic \
		-append "root=LABEL=rootfs console=ttyS0" \
		-fsdev local,id=virtfs0,path=../,security_model=mapped-xattr \
		-device virtio-9p-pci,fsdev=virtfs0,mount_tag=share

# `ssh` rules.
ssh-x86-64:
	ssh -p 2222 root@127.0.0.1

ssh-riscv64:
	ssh -p 2223 root@127.0.0.1
