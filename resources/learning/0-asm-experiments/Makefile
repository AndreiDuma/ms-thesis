AS := as
LD := ld
ASFLAGS := -g
LDFLAGS := -g

# Source and output directories
SRCDIR := src
BINDIR := bin

# List of assembly source files
SRCS := $(wildcard $(SRCDIR)/*.s)

# List of executables
EXECS := $(patsubst $(SRCDIR)/%.s,$(BINDIR)/%,$(SRCS))

# Default target
all: $(EXECS)

# Rule for assembling .s files and linking to executables
$(BINDIR)/%: $(SRCDIR)/%.s
	$(AS) $(ASFLAGS) -o $(BINDIR)/$*.o $<
	$(LD) $(LDFLAGS) -o $@ $(BINDIR)/$*.o

# Create directories if they don't exist
$(shell mkdir -p $(BINDIR))

# Clean target to remove object files and executables
clean:
	rm -rf $(BINDIR)
