find . -type d -name "bin" -exec rm -rf {} +
find . -type d -name "obj" -exec rm -rf {} +
rm -r Godot/.godot/
cd Source
dotnet clean
cd ../Godot
dotnet clean
