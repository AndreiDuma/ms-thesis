riscv64-debian:
	qemu-system-riscv64 \
		-machine 'virt' \
		-cpu 'rv64' \
		-m 4G \
		-device virtio-blk-device,drive=hd \
		-drive file=dqib_riscv64-virt/image.qcow2,if=none,id=hd \
		-device virtio-net-device,netdev=net \
		-netdev user,id=net,hostfwd=tcp::2223-:22 \
		-bios extras/fw_jump.elf \
		-kernel extras/uboot.elf \
		-object rng-random,filename=/dev/urandom,id=rng \
		-device virtio-rng-device,rng=rng \
		-nographic \
		-append "root=LABEL=rootfs console=ttyS0" \
		-fsdev local,id=virtfs0,path=../../,security_model=mapped-xattr \
		-device virtio-9p-pci,fsdev=virtfs0,mount_tag=share

