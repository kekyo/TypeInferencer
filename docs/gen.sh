#!/bin/sh

pandoc HowToPlay_en.md --reference-links --reference-location=block -t html5 -c github.css -o HowToPlay_en.pdf
pandoc HowToPlay_ja.md --reference-links --reference-location=block -t html5 -c github.css -o HowToPlay_ja.pdf
