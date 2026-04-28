#!/bin/bash
# Compile et lance le jeu sous Linux via mono.
# Usage: ./run-mono.sh

set -e
cd "$(dirname "$0")"

echo "─── Compilation avec mcs ───"
mcs -target:winexe -out:Course/Course.exe \
    -r:System.Windows.Forms.dll \
    -r:System.Drawing.dll \
    Course/Program.cs \
    Course/Models/Voiture.cs \
    Course/Forms/SelectionForm.cs \
    Course/Forms/GameForm.cs \
    Course/Controls/Speedometer.cs

echo ""
echo "─── Lancement ───"
cd Course
exec mono Course.exe
