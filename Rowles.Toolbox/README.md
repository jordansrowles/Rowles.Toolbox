
# Text Tools

| Tool                  | Description                                                                       |
| ----------------------- | ----------------------------------------------------------------------------------- |
| Text Counter          | Character, word & sentence statistics, average reading time                       |
| Lorem Ipsum Generator | Generates human-like boilerplate text.                                            |
| Readability           | Flesch and Flesch-Kincaid reading level comprehension scores on text              |
| Escape/Unescape       | Escape & unescape strings for different contexts                                  |
| Unicode Inspector     | Inspect codepoints, UTF-8 bytes & categories on individual characters in a string |
| Levenshtein           | Calculate edit distance & similarity between two strings                          |
| Reverse Text          | Reverse strings, lines, or word order                                             |
| Find & Replace        | Search and replace text with regex support                                        |
| Merge & Split         | Merge multiple texts or split text by delimiter                                   |
| Duplicates            | Find and remove duplicate lines or sentences                                      |

- Text Comparison Tool - Similarity between texts.
- Diff Viewers
    - Side-by-side / unified line diff between two text inputs. Highlight added, removed, changed lines
    - Semantic diff of two JSON documents — highlights structural changes, not just string changes
    - Same as JSON Diff but for YAML
    - Node-level diff of two XML documents
    - Diff two stylesheets, grouping by selector
- Basic Sentiment Detector - Text tone analysis.
- Reference Citation Formatter - MLA APA formats.
- Bibliography Organizer - Manages references.
- ASCII block text generator
- Homoglyph Detector: Identify visually similar Unicode characters substituted for ASCII (e.g. а vs a) — useful for IDN homograph attacks


# Encoding Tools

| Tool           | Description                                     |
| ---------------- | ------------------------------------------------- |
| Base64         | Encode & decode Base64 text (UTF-8)             |
| URL Encode     | Encode & decode URL strings. Component or full. |
| Image Base64   | Convert images to/from Base64 data URIs         |
| JWT Decoder    | Decode & inspect JSON Web Tokens                |
| Hash Generator | Generate hashes for text or files               |
| Password Tools | Generate & analyse passwords                    |

- CRC Calculator: Compute CRC-8, CRC-16, CRC-32 checksums on text or hex input
- ✅ Morse Code Translator: Encode/decode text ↔ Morse code with audio playback option
- ✅ Endianness Converter: Swap byte order for 16/32/64-bit integers, visualize little vs big endian
- ✅ Varint Inspector: Encode/decode protobuf-style variable-length integers
- ✅ UTF-16/UTF-32 Inspector: Show UTF-16 and UTF-32 byte sequences for any string (complements existing UTF-8 tool)
- ✅ IEEE 754 Inspector: Visualize how floats and doubles are represented in binary (sign, exponent, mantissa). Show conversion steps.
- ✅ Two's Complement: Convert signed/unsigned integers to binary and back
- ✅ Binary Diff: Upload two files and show a byte-level diff with offset, old/new byte, and change summary. MemoryStream comparison
- QR Code Generator
- Barcode generator: Generate Code-128, EAN-13, or QR codes


# Data Format Tools

| Tool                   | Description                                                 |
| ------------------------ | ------------------------------------------------------------- |
| YAML ↔ JSON Converter  |                                                             |
| JSON Prettify / Minify |                                                             |
| Query Params ↔ JSON    | Convert between URL query strings and JSON                  |
| Timestamp              | Convert between Unix timestamps and human-readable dates    |
| Number Base            | Convert numbers between bases (2–36)                        |
| Memory Size            | Convert between memory/storage size units                   |
| Scientific             | Convert between standard and scientific notation            |
| Hex Viewer             | Classic hex dump with offset, hex bytes, and ASCII          |
| HTML → Markdown        |                                                             |
| Byte Array             | Parse, inspect, and convert byte arrays between formats     |
| MessagePack            | Decode and inspect MessagePack binary data                  |
| Protobuf               | Decode Protocol Buffers wire format without a .proto schema |
| BSON / JSON Converter  |                                                             |

# Generator Tools

| Tool           | Description                                                |
| ---------------- | ------------------------------------------------------------ |
| UUID Generator | Generate v4 & v7 UUIDs with formatting options             |
| Cron Builder   | Visual cron expression builder with next-run preview       |
| CSS Generator  | Box shadow, border & gradient generators with live preview |
| SVG Gradient   | Create linear & radial SVG gradients with live preview     |
| Markdown Table | Build tables visually or import CSV/TSV                    |

# Colour Tools

| Tool              | Description                                              |
| ------------------- | ---------------------------------------------------------- |
| Colour Converter  | Convert between HEX, RGB, HSL, HSV & CMYK                |
| WCAG Constrast    | Check colour contrast against WCAG 2.1 guidelines        |
| Palette Generator | Generate shades, tints, and harmonies from a base colour |

- Colour Blindness Simulator: Apply deuteranopia / protanopia / tritanopia filters to an uploaded image client-side via Canvas, or over an iframe
- Colour Contrast Matrix: Given a palette, generate a full N×N contrast ratio grid showing all WCAG pass/fail combinations
- Indexed Palette Reducer: Reduce an image to N colours (4/8/16/32/256) using median-cut or k-means, preview dithered result — useful for retro/limited palette art
- Palette Distance Calculator: Input two colour palettes, compute perceptual distance (CIEDE2000) between each pair — identify redundant colours
- Colour Ramp Interpolator: Define two or more key colours, generate a smooth N-step gradient with linear, HSL, Oklab, or perceptual interpolation

# Maths Tools

| Tool             | Description                                        |
| ------------------ | ---------------------------------------------------- |
| Unit Converter   | Convert between units of measurement               |
| Ratio/Percentage | Various percentage, ratio & proportion calculators |
| Time Tools       | Date, duration & time calculators                  |

- Add the following the /maths/units:
    - Density calculator - Mass volume density
    - Torque calculator/converter
    - Power calculator: Watts, volts, amps, ohms (P=VI, P=I²R, P=V²/R)
    - Work calculator
    - Angle unit calculator (Degrees - Radians)
    - Astronomy Unit Converter - Space measurement units.
    - Significant Figures Calculator: Perform arithmetic with correct sig fig rounding rules
- Function Graph Plotter - Plots math functions.
- Complex Number Calculator - Complex arithmetic.
- Trig Function Visualiser - Interactive trig graphs.
- Vector Calculator: 2D and 3D vectors — dot product, cross product, magnitude, angle between, normalise
- Electronics category
    - ✅ Calculators for Ohm's Law, resistence, capacitance, inductance. 
    - ✅ Wave property calculator.
- Science category:
    - ✅ Projectile motion calculator - Computes trajectory.
    - ✅ Kinematics Equation Solver - Solves motion problems.
    - ✅ Molar Mass calculator - For chemical compounds
    - pH Level estimator - Computes pH Level?
    - Chemical equation balancer - Balances reactions
    - Error / Uncertainty Propagation: Propagate absolute and relative uncertainties through addition, multiplication, and powers
    - Chemistry:
        - Empirical Formula Calculator: Input mass percentages of elements and derive the empirical and molecular formula
        - Stoichiometry Calculator: Given a balanced equation and a known quantity, compute moles/grams of all other species
        - Ideal Gas Law Calculator: Solve for P, V, n, or T given the other three values (PV = nRT)
        - Thermochemistry Calculator: Calculate ΔH for a reaction using Hess's Law from formation enthalpies
        - Molarity / Dilution Calculator: Compute concentration, volume, or moles; C₁V₁ = C₂V₂ dilution solver
        - Henderson-Hasselbalch Calculator: Compute buffer pH from pKa and acid/conjugate base ratio
        - Titration Curve Plotter: Plot a strong/weak acid-base titration curve given volume, concentration, and Ka
        - Oxidation State Assigner: Input a compound and assign oxidation states to each element
        - Electrochemistry Calculator: Compute cell potential (E°cell), Gibbs free energy (ΔG), and equilibrium constant (K)
        - Half-Life / Radioactive Decay: Compute remaining quantity, activity, or time elapsed for first-order nuclear decay

# Network Tools

| Tools          | Descriotions                                               |
| ---------------- | ------------------------------------------------------------ |
| CIDR/Subnet    | Calculate subnets, hosts, and IP ranges from CIDR notation |
| Port Reference | Well-known and common service port numbers                 |

- IPv4 IPv6 Converter: Convert IPv4 addresses to IPv4-mapped IPv6 notation and back
- IPv6 Expander/Compressor: Convert IPv4 addresses to IPv4-mapped IPv6 notation and back
- Supernet Calculator: Aggregate multiple CIDR blocks into the smallest enclosing supernet
- ✅ SPF/DKIM/DMARC Builder trio — Visual wizard to build a valid v=spf1 TXT record with mechanism reference. Paste a DKIM public key TXT record and decode the p= base64 key and tag fields. Compose and validate a _dmarc TXT record with policy, pct, rua, and ruf guidance.
- ✅ TCP/IP Header Dissector: Paste raw hex bytes and parse IPv4/TCP/UDP header fields with field-by-field annotation
- ✅ Ethernet Frame Inspector: Decode an Ethernet II frame from hex — dst/src MAC, EtherType, payload offset
- ✅ curl Command Builder: Build curl commands visually — method, headers, body, auth, flags — with copy output
- ✅ Nmap Flag Builder: Compose nmap scan commands visually with flag explanations and security warnings

# Image Tools

| Tool             | Description                                                       |
| ------------------ | ------------------------------------------------------------------- |
| Resize/Crop      | Resize, convert & compress images client-side                     |
| SVG Viewer       | Preview, inspect & export SVG images                              |
| WebP Convert     | Convert images to/from WebP format                                |
| EXIF Inspector   | Extract & view EXIF metadata from JPEG/TIFF images                |
| Extract Palette  | Extract dominant colours from any image                           |
| Add Watermark    | Add a text watermark to any image client-side                     |
| Watermark Check  | Detect potential visible watermarks in images                     |
| Format Inspector | Inspect raw bytes to identify image format, dimensions & metadata |
| SVG Path         | Parse & visualise SVG path commands                               |
| Code Screenshot  | enerate beautiful code screenshots client-side                    |

- Image Grayscaler, Image Rotator, Image Flipper, Image Compressor
- GIF Generator
- Video File Header Inspector: Parse MP4/MKV/WebM container metadata — duration, codec, resolution, frame rate — without playing. Parse ftyp/moov atoms via BinaryReader
- PNG Chunk Inspector: Parse all PNG chunks (IHDR, PLTE, tEXt, iCCP, gAMA, tIME) with field-level breakdown. PNG spec byte parsing
- JPEG Segment Inspector: Walk JPEG markers (SOI, APP0–APP15, SOF, DHT, SOS) and display segment lengths and content. JPEG marker parsing
- Steganography Detector: Analyse an image for LSB anomalies and flag statistically suspicious pixel distributions Canvas pixel data via JS interop
- EXIF GPS Extractor: Extract GPS lat/lon from EXIF and plot on a static map tileExtends your existing EXIF Inspector

# Developer Tools

| Tool            | Description                                                      |
| ----------------- | ------------------------------------------------------------------ |
| Conn Strings    | Reference & builder for common connection strings                |
| Big O           | Algorithm complexity cheat sheet                                 |
| Error Codes     | Searchable reference for HTTP, gRPC, Win32 & HRESULT codes       |
| Stack Trace     | Parse & format stack traces from .NET, Java, Node.js, and Python |

- ✅ Mermaid diagram renderer: Render Mermaid.js markup — flowcharts, sequence, ER, Gantt
- ✅ PlantUML preview: Render PlantUML diagrams via the public PlantUML server
- Conventional Commit Builder: Visual builder for conventional commit messages (feat(scope): desc) with type reference
- README Badge Generator: Visual builder for shields.io badge Markdown — status, version, license, custom labels


# .NET Developer

| Tool            | Description                                                      |
| ----------------- | ------------------------------------------------------------------ |
| .NET Regex      | Test, debug & replace with .NET regular expressions              |
| DateTime        | .NET  DateTime & DateTimeOffset playground                       |
| Culture Format  | Explore .NET CultureInfo number & date formatting                |
| Stack Trace     | Parse & format stack traces from .NET, Java, Node.js, and Python |
| BDN Formatter   | Format and analyse BenchmarkDotNet results                       |
| LINQ Tester     | Build & test LINQ queries on sample collections                  |
| Expression Tree | Visualize .NET expression trees as interactive diagrams          |
| Func Builder    | Build Func, Action, Predicate & delegate signatures              |

- A tool to take multiple `.csproj` files, and create a `Directory.Packages.props' and return the edited project files. (Nuget Package Consolidation)
- ✅ C# String Interpolation Tester: Preview string.Format / interpolation output with live substitution
- ✅ Struct Layout Visualiser: Input C# struct fields + types, show sequential memory layout, padding, total size (like [StructLayout] inspector)
- ✅ C# Type Hierarchy Browser: Paste a type name, display its inheritance chain and implemented interfaces (static dataset from BCL)
- ✅ Span\<T> Slice Bounds Calculator: Input buffer length, offset, and count — validate slice params and show resulting range and edge cases

# Web Development

| Tool            | Description                                                      |
| ----------------- | ------------------------------------------------------------------ |
| Regex Tester   | Test and debug regular expressions with live matching      |
| HTTP Status    | HTTP status codes & MIME types reference                   |
| HTTP Header    | Build and parse HTTP request/response headers              |
| User Agent     | Parse and analyse browser user agent strings               |
| Screen Sizes   | Common device screen sizes and responsive breakpoints      |
| Web Linters     | Lint HTML, CSS & JavaScript for common issues                    |
| Web Units        | Convert between CSS & web units                    |
| DOM Tree     | Parse & visualize HTML DOM structure                 |
| XPath Tester | Evaluate XPath expressions against XML documents     |
| JSONPath     | Query JSON data using JSONPath expressions           |
| Flexbox      | Interactive CSS flexbox playground with live preview |

- CSS Grid Builder - interactive CSS grid playground with live preview
- ✅ CSS Keyframe Animator: Build CSS keyframe animations visually
- WebSocket tester - Connect to WebSocket endpoints and send/receive frames ?
- SSE / EventSource tester - Connect to Server-Sent Events endpoints and display the event stream
- ✅ C`robots.txt` Builder and validator
- ✅ CHTML entity reference/search - Searchable named HTML entity reference (&amp;, &nbsp;, etc.)
- ✅ CWeb Font previewer - Preview Google Fonts and system fonts on user-entered text
- ✅ CFavicon generator - Generate a full favicon set (16px–512px) from an uploaded image
- ✅ CDevice fingerprint breakdown
- ✅ CManifest.json Builder: Build a PWA manifest.json with all required fields and icon slot guidance
- ✅ .htaccess Snippet Builder: Build common Apache .htaccess rules — redirects, HTTPS enforcement, compression
- ✅ CSVG Icon Sprite Builder: Paste multiple SVGs and merge into a <symbol>-based sprite sheet
- ✅ CSS Print Builder: Build @media print override rules for controlling page breaks, headers, footers
- ✅ CSS Counters Builder: Visual builder for CSS counter-increment / counter-reset list numbering schemes
- ✅ CSS Filter Builder: Visual builder for filter: blur/brightness/contrast/hue-rotate/saturate chains
- ✅ CSS Transform Visualiser: Interactive transform builder — rotate, scale, skew, translate, perspective
- JS AST Explorer: Parse a JS snippet and visualise the Abstract Syntax Tree as an interactive collapsible tree
- Prototype Chain Inspector: Input a JS object literal and display the full `[[Prototype]]` chain up to Object.prototype
- Event Loop Visualiser: Step through a code snippet and animate the call stack, task queue, and microtask queue
- Scope Chain Visualiser: Paste a JS function and visualise closure scopes, variable hoisting, and lexical environment nesting


# File Tools

| Tool           | Description                                          |
| ---------------- | ------------------------------------------------------ |
| HAR Viewer     | Inspect HTTP Archive (.har) files                    |
| CSV Viewer     | View & filter CSV, TSV, and delimited files          |
| File Signature | Identify file types by magic bytes / file signatures |
| Checksum       | Calculate & verify file checksums                    |
| Encoding/BOM   | Detect encoding, BOM, and line endings               |

- PDF Metadata Inspector: Extract title, author, creation date, page count, and producer from a PDF without rendering it. Parse PDF header/xref via MemoryStream; no render needed
- DOCX Metadata Extractor: Extract author, word count, revision count, and custom properties from .docx [Content_Types].xml. DOCX is a ZIP; parse with ZipArchive.
- Excel Sheet Inspector: List sheet names, row/column counts, and named ranges from .xlsx without full parsing. Same ZIP approach on xl/workbook.xml
- EPUB Inspector: List chapters, metadata, and spine order from an .epub file (which is a ZIP). ZipArchive on content.opf
- PDF Digital Signature Inspector: Detect and display embedded digital signatures and certificate chains in a PDF. Parse /Sig dictionary from PDF bytes

- ZIP / Archive Inspector: List entries, sizes, compression ratios, and paths inside a .zip without extracting. System.IO.Compression.ZipArchive works in WASM
- ZIP Creator: Drag in multiple files and download them as a .zip archive. System.IO.Compression.ZipArchive in create mode
- GZip Compress / Decompress: Compress or decompress .gz files or raw byte streams. GZipStream available in WASM
- Deflate / Zlib Playground: Compress text with Deflate/Zlib and compare output size, ratio, and hex. DeflateStream / ZLibStream
- Archive Comparison: Upload two archives and diff their contents — added, removed, changed entries by filename and size. ZipArchive on both


# Security Tools

| Tool           | Description                                     |
| ---------------- | ------------------------------------------------- |
| Password Tools | Generate & analyse passwords                    |

- CORS Header Builder - Build Access-Control-* response headers with visual toggle
- CSP Builder - Content Security Policy header builder, validator, and directive reference
- Bcrypt Tool - Hash and verify bcrypt passwords entirely client-side
- ✅ DNS-over-HTTPS Lookup - Query A, AAAA, MX, TXT, CNAME records via DoH API
- ✅ Cipher playground, a page that includes the following:
    - ROT13 / Caesar Cipher: Apply ROT13, ROT47, or custom Caesar shifts to text
    - Vigenère Cipher: Encode/decode text using a keyword-based Vigenère cipher
    - Atbash Cipher: Reverse-alphabet cipher (A↔Z, B↔Y, etc.)
    - XOR Encryptor: Encrypt/decrypt text using a repeating XOR key
    - AES-GCM Simulator: Client-side AES-GCM encryption/decryption (Web Crypto API)
    - HMAC Generator: Generate HMAC-SHA256/512 with secret key
    - Random Bytes Generator: Generate cryptographically random bytes/hex/base64

# Audio Tools

- Audio File Inspector: Extract codec, duration, sample rate, bit depth, and channel count from WAV/FLAC/OGG headers. Parse binary headers via BinaryReader.
- MP3 ID3 Tag Reader: Read ID3v1 and ID3v2 tags — title, artist, album, track, cover art. Binary parsing, ID3 spec is well-documented
- Waveform: Visualize audio file waveform (client-side Web Audio)
- Trimmer: Trim audio files to start/end times
- Normaliser: Normalize peak volume of audio files
- Converter: Convert between WAV, MP3 (demo), OGG (browser limits)
- Spectogram viewer: Generate spectrogram from audio file
- BPM counter: Tap or upload audio to estimate beats per minute

# Misc

- Coin flipper/variable dice roller (simple 6-sided, or multiple difference dies) with probability stats

# Game Dev Tools

## Sprite & Textures

- Sprite Sheet Packer: Upload multiple images, pack them into a single atlas with configurable padding/power-of-two sizing, export the sheet + JSON manifest
- Sprite Sheet Slicer: Upload an atlas + JSON/XML metadata (Unity, Aseprite, TexturePacker formats), preview and export individual frames
- Pixel Art Scaler: Scale pixel art images using nearest-neighbour, EPX/Scale2x, or HQx algorithms — not bicubic which blurs pixels
- Sprite Palette Swapper: Replace one colour palette with another on a spritesheet — useful for character recolours/team skins
- Normal Map Generator: Generate a tangent-space normal map from a greyscale heightmap image — client-side Canvas processing
- Texture Channel Splitter / Merger: Split an RGBA image into R/G/B/A channel images and recombine — common for PBR packed textures (metallic/roughness/AO)
- Alpha Mask Tool: Apply or extract an alpha mask from an image — make a colour channel the alpha, or bake alpha to white/black

## Map & Tileset

- Tileset Grid Inspector - Upload a tileset image, define tile size, preview the grid overlay with tile index numbers — validate your atlas before importing
- Tileset Duplicate Finder - Scan a tileset for visually identical tiles to trim atlas size — pixel-exact comparison
- Procedural Noise Visualiser - Visualise Perlin, Simplex, and Worley noise with adjustable octaves, frequency, and lacunarity — export as greyscale heightmap PNG
- Hex Grid Calculator - Cube/axial/offset coordinate converter, neighbour finder, distance and line-of-sight calculator for hex grids
- Pathfinding Visualiser - Interactive grid with A*, Dijkstra, BFS, Greedy step-through — place walls, move start/end, watch frontier expand

## Audio Tools

- ADSR Envelope Visualiser: Adjust Attack/Decay/Sustain/Release sliders, see the amplitude curve, hear a test tone — useful for understanding synth patches
- Audio Loop Point Finder: Visualise a waveform and set loop start/end points, preview the seamless loop — critical for background music
- Sound Effect Similarity Scorer: Upload two short audio clips, compute waveform correlation score — helps avoid repetitive SFX
- 8-bit / Chiptune Tone Generator: Generate square, triangle, sawtooth, and noise waveforms at target frequencies with duration — export as WAV

## Animation

- Easing Function Visualiser: Plot all CSS/common easing curves (ease-in-out, elastic, bounce, back) side by side with a moving preview ball — export curve as a lookup table array
- Spritesheet Animator Preview: Upload a spritesheet, define frame size and FPS, preview the animation — no code, no engine
- Bone Hierarchy Visualiser: Define a skeleton as a JSON parent-child hierarchy, render it as an interactive tree and stick-figure diagram
- Keyframe Interpolation Calculator: Input two keyframe values + easing, get the interpolated value at any normalized time t — useful for hand-rolled animation code

## Maths

- Loot Table Probability Analyser: Define items with weights, compute drop rates and expected drops per N runs — shows compounding rarity
- Damage Formula Tester: Define a formula with variables (ATK, DEF, LVL), sweep variable ranges, plot output as a heatmap or curve
- Procedural Name Generator: Define syllable tables and phonetic rules, generate names — export a seeded list

## Geometry & Physics

- 2D Collision Shape Debugger: Define AABB, circle, and convex polygon shapes, test intersection and show contact normals
- Bezier Curve Editor: Interactive cubic / quadratic Bézier editor, export as SVG path or point array for use in spline-based movement
- Trigonometry for Games: Interactive unit circle showing sin/cos/atan2 with a game-framing — "angle to target", "component velocity" converters
- Physics Constants Reference: Searchable table of constants (g, c, μ₀, etc.) useful for physics simulations, with unit variants
- Polygon Triangulator: Input a polygon as vertex list, triangulate using ear-clipping, visualize triangle fan — useful for custom mesh generation

## Assets
- Unity .meta File Inspector: Parse a Unity .meta YAML file and display the GUID, asset type, and importer settings in a readable form
- Aseprite JSON Manifest ParserParse and display Aseprite's exported JSON frame manifest — frame names, durations, tags, slice keys
- Tiled Map (.tmx) Inspector: Parse a Tiled .tmx XML file, display layer structure, tileset references, object groups, and properties
- Asset Dependency Graph: Paste a list of asset filenames with references, render a DAG of dependencies — identify circular refs
- AssetFile Size Budget Calculator: Define a platform target (mobile, console, PC), allocate budget across asset categories, track total vs budget with a breakdown chart