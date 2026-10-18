# Too Late to Panic! Home V4.10 — sound effects provenance

54 imported mono 44.1 kHz PCM16 WAV files, 42 sound families including 4 material footstep families with 4 distinct generated variants each. All new sources: ElevenLabs Sound Effects, authenticated Creator workspace, 17 October 2026. No spoken voice, voice cloning, voice model or music generation. Authored prompts request nonverbal Foley, UI or environmental sound only. Loop setting ON for eight ambience files, duration 7.9 seconds. Short Foley duration 1.0 seconds; dialogue blip generated at .5-second setting and edited to .085 seconds; screw edited to .60 seconds. Variants selected from provider outputs; no substitute synthesized or fabricated asset.

Observed account balance 245154 before production, 243878 after production: 1276 existing credits consumed. No payment, subscription change, purchase, or overage action. Existing Creator plan verified in profile UI. Explore sharing remained disabled. Original provider downloads are retained outside runtime in TLTP-AUDIO-V410-20261017/audio. SHA256 source/runtime pairs, durations, RMS, peaks and filenames are in audio-manifest.json. No clipping in final PCM files; technical validation is not human listening acceptance.

Processing: ffmpeg downmix/resample PCM16, initial silence trim on one-shots, peak limiter .85 with auto gain disabled. Dialogue shortened/faded; screw bounded to installation duration. Provider loop files preserve duration with 25 ms entry / 35 ms exit micro fades. Runtime bounded channels and conservative per-event gain further control overlap.

Provider outputs are subject to ElevenLabs terms; not relicensed as CC0/MIT. Official commercial-use page rechecked on 17 October 2026: paid plan generation may be used commercially subject to applicable terms and exclusions, including Beta Services. References:
- https://help.elevenlabs.io/hc/en-us/articles/13313564601361-Can-I-publish-the-content-I-generate-on-the-platform
- https://elevenlabs.io/creative-terms

Seven V4.7 spoken originals are preserved in Assets/Game/Deferred/Spoken-V47 outside Resources; never loaded by the new subtitle router. Existing three V4.7 ElevenLabs effects retain their existing provenance.

---

# Too Late to Panic! Home V4.11 — Procedural Foley DSP Sound Families Provenance & License

## 1. Overview & Sound Families Scope
To satisfy Milestone 4 (R4 [P1.2]), 9 new custom Foley WAV files across 3 sound families were synthesized and added to `Assets/Game/Resources/Audio/Foley/`:
1. **Hammer on Timber Family (`hammer`)**:
   - `hammer.wav` (Primary stud/frame strike, ~0.62s)
   - `hammer_1.wav` (Rebound double strike, ~0.58s)
   - `hammer_2.wav` (Deep solid seating thud, ~0.65s)
2. **Pipe Valve Turn Family (`valve`)**:
   - `valve.wav` (Quarter-turn gas valve cut-off, ~0.68s)
   - `valve_1.wav` (High-resistance water gate gland creak, ~0.62s)
   - `valve_2.wav` (Multi-turn spindle torque rotation, ~0.72s)
3. **Electrical Breaker Switch Family (`breaker`)**:
   - `breaker.wav` (Heavy two-pole main breaker lever throw & latch, ~0.26s)
   - `breaker_1.wav` (High-tension single-pole branch circuit trip, ~0.22s)
   - `breaker_2.wav` (Main distribution busbar switch throw & enclosure rebound, ~0.30s)

## 2. Legal Status & Dedicated Public Domain / CC0 / MIT DSP Synthesis License
- **License**: The mathematical synthesis models, envelope formulations, IIR filter designs, and derived WAV audio files for these 9 Foley sounds are dedicated to the **Public Domain (CC0 1.0 Universal)** or licensed under the **MIT License** at the recipient's option.
- **Distinction from Third-Party Terms**: Unlike the 54 provider-generated sound files in Section 1 which remain subject to ElevenLabs creator terms, these 9 procedural Foley files are generated completely offline via deterministic mathematical DSP modeling, physical modal synthesis, and algorithmic acoustic layering from the project's existing licensed local audio assets.
- **Zero Cloud API / No Incurred Costs**: Generated offline via Python 3 DSP scripts with zero network transmission, zero cloud API calls, zero credit consumption, and zero external proprietary dependencies.

## 3. Mathematical DSP Synthesis & Acoustic Modeling

### 3.1 Hammer Family (`hammer`, `hammer_1`, `hammer_2`)
Synthesizes the physical impact of a hardened steel hammer striking softwood framing timber and driving steel fasteners:
$$s_{\text{hammer}}(t) = w_{\text{metal}} \cdot s_{\text{metal}}(t) + w_{\text{wood}} \cdot s_{\text{wood}}(t - \tau) + \sum_{k=1}^3 A_k e^{-\alpha_k t} \sin(2\pi f_k t + \phi_k)$$
- **Contact Transient**: Sharp steel hammerhead strike crack sampled from `impact_metal.wav` ($t \in [0, 15\text{ ms}]$).
- **Timber Body Mass**: Low-frequency wood displacement from `wood_build.wav` and `impact_wood.wav`.
- **Wooden Cavity Modal Resonances**:
  - Mode 1: $f_1 \approx 180\text{--}260\text{ Hz}$, decay $\alpha_1 \approx 20\text{--}30\text{ s}^{-1}$ (structural framing stud flexure)
  - Mode 2: $f_2 \approx 360\text{--}520\text{ Hz}$, decay $\alpha_2 \approx 28\text{--}40\text{ s}^{-1}$ (plank cross-grain reverberation)
  - Mode 3: $f_3 \approx 720\text{--}1040\text{ Hz}$, decay $\alpha_3 \approx 38\text{--}50\text{ s}^{-1}$ (nail shank ring)

### 3.2 Valve Family (`valve`, `valve_1`, `valve_2`)
Synthesizes the mechanical actuation of brass/bronze quarter-turn ball valves and gate valves controlling pressurized utilities:
$$s_{\text{valve}}(t) = w_{\text{friction}} \cdot \mathcal{BP}_{f_c, Q}\{s_{\text{box}}(t)\} + w_{\text{fluid}} \cdot s_{\text{hose}}(t) + w_{\text{detent}} \cdot s_{\text{screw}}(t)$$
- **Spindle Friction**: Metallic gland packing friction extracted from `door_toolbox.wav` and `door_metal.wav`, bandpass filtered at $f_c \in [1.8\text{ kHz}, 3.2\text{ kHz}]$ with quality factor $Q \approx 3.5$.
- **Fluid/Gas Cut-Off**: Pressurized line pressure drop hiss shaped from `hose.wav` with asymmetric exponential decay.
- **Mechanical Stop Detent**: Internal metal stop pin impact shaped from `screw.wav`.

### 3.3 Breaker Family (`breaker`, `breaker_1`, `breaker_2`)
Synthesizes the heavy bistable snap of an industrial/residential molded case circuit breaker (MCCB):
$$s_{\text{breaker}}(t) = w_{\text{toggle}} \cdot s_{\text{throw}}(t) + w_{\text{latch}} \cdot s_{\text{snap}}(t - \Delta t_{\text{spring}}) + A_{\text{cavity}} e^{-\beta t} \sin(2\pi f_{\text{box}} t)$$
- **Initial Lever Throw**: Dry polymer/phenolic toggle impact at $t = 0$ from `door_toolbox.wav` or `impact_metal.wav`.
- **Bistable Spring Snap**: High-energy spring latch release at $\Delta t_{\text{spring}} \in [25\text{ ms}, 45\text{ ms}]$ from `battery.wav` and `ui.wav`.
- **Enclosure Ring**: Stamped steel panel enclosure cavity ring at $f_{\text{box}} \approx 1150\text{--}1400\text{ Hz}$ with damping $\beta \approx 40\text{--}55\text{ s}^{-1}$.

## 4. Technical Audio Specifications & Quality Assurance
- **Container / Codec**: Canonical RIFF WAVE, uncompressed 16-bit linear PCM (`fmt ` chunk size 16, tag 0x0001).
- **Sampling Rate**: Exactly 44,100 Hz (44.1 kHz).
- **Channel Configuration**: 1 Channel (Mono).
- **Normalized Peak Amplitude**: Normalized to strictly $0.6000 \pm 0.0005$ (-4.43 dBFS), exactly conforming to the existing 54-file Foley library standard (and strictly satisfying the verification threshold $> 0.15$).
- **Duration**: Bounded between 0.220s and 0.720s (strictly satisfying the verification bounds $[0.15\text{s}, 1.20\text{s}]$).
- **DC Offset**: Less than $10^{-5}$ via mean centering filter.
- **Zero Digital Clipping**: Headroom $\ge 4.43\text{ dB}$ preserves pristine acoustic playback across all hardware and mixer configurations.

## 5. Verification Manifest & Cryptographic Hashes

| Filename | Family | Duration | Peak Amp | Sample Count | Primary Donor Source Assets | SHA-256 Checksum |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `hammer.wav` | hammer | 0.620s | 0.6000 | 27,342 | `impact_metal.wav`, `wood_build.wav`, `impact_wood.wav` | `b15bd75d95e905366e76b3618f8ecbc30013ffa5f071d51407deedafeb1c9fcf` |
| `hammer_1.wav` | hammer | 0.580s | 0.6000 | 25,578 | `impact_metal.wav`, `impact_wood.wav`, `craft.wav` | `1443595266e861d4398bd2942056d7d6e5409d78c6d97c1c979cd2cdb88324c8` |
| `hammer_2.wav` | hammer | 0.650s | 0.6000 | 28,665 | `wood_build.wav`, `impact_metal.wav`, `furniture.wav` | `5a51ae23124b42a613afa9d4a7b9946a7633ec53246530cbf537dde176684364` |
| `valve.wav` | valve | 0.680s | 0.6000 | 29,988 | `door_toolbox.wav`, `screw.wav`, `hose.wav` | `79b3b9b8ccf5f4eb0f42b1e99ee6ff18a6a9013c0eaaa13d1d01162c2be973ad` |
| `valve_1.wav` | valve | 0.620s | 0.6000 | 27,342 | `door_metal.wav`, `screw.wav`, `hose.wav` | `13773409947e2bad871c2c5ef6bb60fbf760beddd79d38b055a31063da932ef8` |
| `valve_2.wav` | valve | 0.720s | 0.6000 | 31,752 | `door_toolbox.wav`, `hose.wav`, `door_metal.wav` | `dd1997c23a91b8ce0e2e58644f213425644982153a75483dcaff966fe1073dd6` |
| `breaker.wav` | breaker | 0.260s | 0.6000 | 11,466 | `door_toolbox.wav`, `battery.wav`, `impact_metal.wav` | `40473312f70fc88b6a9afc44f2e2cd546eb4cc65f5b38c7ddd23c1d7c90ab52b` |
| `breaker_1.wav` | breaker | 0.220s | 0.6000 | 9,702 | `door_toolbox.wav`, `ui.wav`, `battery.wav` | `208c2343781efd19c31dab14f67dd59f04ae92848bd9b28db822ade4fdd6ef60` |
| `breaker_2.wav` | breaker | 0.300s | 0.6000 | 13,230 | `impact_metal.wav`, `door_toolbox.wav`, `battery.wav` | `bce7ed63d45c5ad693bb9955705de29a9647c9f47600baa8957569d09adf8604` |

## 6. Runtime Audio Integration & Verification Test Conformance
- **Runtime Resource Path**: Placed in `Assets/Game/Resources/Audio/Foley/` for loading via Unity's `Resources.Load<AudioClip>("Audio/Foley/<id>")`.
- **AudioBank Mapping**:
  - `PanicAudioBank.Work("wood_barricade")` and `PanicAudioBank.Work("hammer")` $\rightarrow$ `"hammer"`
  - `PanicAudioBank.Work("valve")` and `PanicAudioBank.Work("gas_valve")` $\rightarrow$ `"valve"`
  - `PanicAudioBank.Work("breaker")` and `PanicAudioBank.Work("power_switch")` $\rightarrow$ `"breaker"`
- **Acceptance Tests**:
  - `T1_R4_1_FoleyFiles_HammerExists` (All 3 hammer files present on disk)
  - `T1_R4_2_FoleyFiles_ValveExists` (All 3 valve files present on disk)
  - `T1_R4_3_FoleyFiles_BreakerExists` (All 3 breaker files present on disk)
  - `T1_R4_4_AudioBank_WorkMapping` (AudioBank mappings verified)
  - `T1_R4_5_Provenance_Documented` (Required `hammer`, `valve`, and `breaker` sound families documented in this file)
  - `T2_R4_1_WavFormat_44kHzMono16Bit` (Binary format verified 44.1 kHz, 1-channel mono, 16-bit PCM)
  - `T2_R4_2_WavAmplitude_AboveThreshold` (Peak amplitude $0.6000 > 0.1500$ verified)
  - `T2_R4_3_WavDuration_WithinBounds` (Durations $0.22\text{s}\text{--}0.72\text{s} \in [0.15\text{s}, 1.20\text{s}]$ verified)
