// AudioManager for C.A.R.E Web

class AudioManager {
  constructor() {
    this.bgm = null;
    this.currentBgmTrack = null;
    this.isMuted = false;
    this.audioContext = null;

    this.soundPaths = {
      click: "assets/audio/sfx_click.mp3",
      whoosh: "assets/audio/sfx_whoosh.mp3",
      hit: "assets/audio/sfx_hit.mp3",
      badge: "assets/audio/sfx_badge.mp3",
      bonus: "assets/audio/sfx_bonus.mp3",
      page: "assets/audio/sfx_page.mp3",
      skip: "assets/audio/tone/skipDialog.ogg"
    };

    this.bgmTracks = {
      title: "assets/audio/bgm_soft_piano.mp3",
      story: "assets/audio/amb_fire.mp3",
      hub: "assets/audio/bgm_nature_hub.mp3",
      combat1: "assets/audio/bgm_combat_26s.mp3",
      combat2: "assets/audio/bgm_battle_60s.mp3",
      combat3: "assets/audio/bgm_boss.mp3"
    };

    this.tones = {
      narrator: "assets/audio/tone/Narrator + MC Tone.mp3",
      sawit: "assets/audio/tone/Normal Tone - Dr Sawit man.mp3",
      sawitAngry: "assets/audio/tone/Angry Tone - Dr Sawit Man.mp3"
    };

    this.initAudioContext();
  }

  initAudioContext() {
    try {
      const AudioCtx = window.AudioContext || window.webkitAudioContext;
      if (AudioCtx) {
        this.audioContext = new AudioCtx();
      }
    } catch (e) {
      console.warn("Web Audio API not supported", e);
    }
  }

  resumeContext() {
    if (this.audioContext && this.audioContext.state === "suspended") {
      this.audioContext.resume();
    }
  }

  playBGM(trackKey, loop = true) {
    if (this.currentBgmTrack === trackKey && this.bgm && !this.bgm.paused) {
      return;
    }

    const path = this.bgmTracks[trackKey] || trackKey;
    if (this.bgm) {
      this.bgm.pause();
      this.bgm.currentTime = 0;
    }

    try {
      this.bgm = new Audio(path);
      this.bgm.loop = loop;
      this.bgm.volume = this.isMuted ? 0 : 0.45;
      this.currentBgmTrack = trackKey;
      this.bgm.play().catch(e => {});
    } catch (e) {
      console.warn("Error playing BGM", e);
    }
  }

  stopBGM() {
    if (this.bgm) {
      this.bgm.pause();
      this.bgm.currentTime = 0;
      this.currentBgmTrack = null;
    }
  }

  playSFX(sfxKey) {
    if (this.isMuted) return;
    this.resumeContext();
    const path = this.soundPaths[sfxKey] || sfxKey;
    try {
      const audio = new Audio(path);
      audio.volume = 0.6;
      audio.play().catch(() => {});
    } catch (e) {
      this.synthBeep(440, 0.05);
    }
  }

  playTypewriterBlip(pitch = 500) {
    if (this.isMuted) return;
    this.resumeContext();
    if (this.audioContext) {
      try {
        const osc = this.audioContext.createOscillator();
        const gain = this.audioContext.createGain();
        osc.type = "sine";
        osc.frequency.setValueAtTime(pitch, this.audioContext.currentTime);
        osc.frequency.exponentialRampToValueAtTime(pitch * 0.8, this.audioContext.currentTime + 0.03);
        gain.gain.setValueAtTime(0.08, this.audioContext.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.001, this.audioContext.currentTime + 0.03);
        osc.connect(gain);
        gain.connect(this.audioContext.destination);
        osc.start();
        osc.stop(this.audioContext.currentTime + 0.03);
      } catch (e) {}
    }
  }

  synthBeep(freq = 440, duration = 0.1) {
    if (!this.audioContext) return;
    try {
      const osc = this.audioContext.createOscillator();
      const gain = this.audioContext.createGain();
      osc.type = "triangle";
      osc.frequency.setValueAtTime(freq, this.audioContext.currentTime);
      gain.gain.setValueAtTime(0.15, this.audioContext.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.01, this.audioContext.currentTime + duration);
      osc.connect(gain);
      gain.connect(this.audioContext.destination);
      osc.start();
      osc.stop(this.audioContext.currentTime + duration);
    } catch (e) {}
  }

  toggleMute() {
    this.isMuted = !this.isMuted;
    if (this.bgm) {
      this.bgm.volume = this.isMuted ? 0 : 0.45;
    }
    return this.isMuted;
  }
}

const audioMgr = new AudioManager();
