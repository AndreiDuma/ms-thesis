AS := as
LD := ld
ASFLAGS := -g
LDFLAGS := -g

# Source and output directories
SRCDIR := src
BINDIR := bin

# List of assembly source files
SRCS := $(wildcard $(SRCDIR)/*.s)

# List of object files
OBJS := $(patsubst $(SRCDIR)/%.s,$(BINDIR)/%.o,$(SRCS))

# Final executable
EXEC := $(BINDIR)/forth

# Default target
all: $(EXEC)

$(EXEC): $(OBJS)
	$(LD) $(LDFLAGS) -o $@ $<

# Rule for assembling .s files and linking to executables
$(BINDIR)/%.o: $(SRCDIR)/%.s
	$(AS) $(ASFLAGS) -o $(BINDIR)/$*.o $<

# Create directories if they don't exist
$(shell mkdir -p $(BINDIR))

# Clean target to remove object files and executables
clean:
	rm -rf $(BINDIR)

gdb: $(EXEC)
	gdb $<
