AudioContext = window.AudioContext || window.webkitAudioContext;
const context = new AudioContext();
let source;
let volumeNode;
let start = 0;
let hasEnded = true;
let currentVolume = 1;

setInterval(() => {
    if (!source?.buffer || hasEnded ||
        context.state === "suspended" ||
        context.state === "closed") {
        return;
    }
    else if (source.buffer.length <= 0) {
        return;
    }

    DotNet.invokeMethodAsync("Mudify.Client", "OnTrackProgress", progress(), false);
}, 100)

window.play = (bytes) => {
    let array = new Uint8Array(bytes);
    context.decodeAudioData(array.buffer, initiate);
}

window.pause = () => {
    suspend();
};

window.unpause = () => {
    resume();
}

window.updateVolume = (volume) => {
    updateVolumeGain(volume);
}

function initiate(buffer) {
    if (!hasEnded && source) {
        dispose();
    }

    volumeNode = context.createGain();
    volumeNode.gain.value = currentVolume;
    volumeNode.connect(context.destination);

    source = context.createBufferSource();
    source.buffer = buffer;
    source.connect(volumeNode);
    source.start();
    start = context.currentTime;
    hasEnded = false;

    DotNet.invokeMethodAsync("Mudify.Client", "OnTrackStart", source.buffer.duration * 1000);
    source.onended = onEnded;
}

function resume() {
    context.resume();
}

function suspend() {
    context.suspend();
}

function onEnded() {
    dispose();
    DotNet.invokeMethodAsync("Mudify.Client", "OnTrackProgress", progress(), true);
}

function progress() {
    return ((context.currentTime - start) / source.playbackRate.value) * 1000;
}

function updateVolumeGain(volume) {
    currentVolume = volume / 100;
    volumeNode.gain.value = currentVolume;
}

function dispose() {
    hasEnded = true;
    source.stop();
    source.disconnect();
    source.onended = null;
}