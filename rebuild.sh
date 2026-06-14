find . -type d -name "bin" -exec rm -rf {} +
find . -type d -name "obj" -exec rm -rf {} +
cd Godot
rm -rf .godot/
dotnet clean
dotnet build
cd ../Source
dotnet clean
