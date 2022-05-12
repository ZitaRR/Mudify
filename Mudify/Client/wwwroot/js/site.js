AudioContext = window.AudioContext || window.webkitAudioContext;
const context = new AudioContext();
const assembly = "Mudify.Client";
const trackStart = "OnTrackStart";
const trackProgress = "OnTrackProgress";
let sourceNode;
let volumeNode;
let startTime = 0;
let currentVolume = 1;

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
    updateVolumeNode(volume);
}

setInterval(() => {
    if (!isAudioPlaying()) {
        return;
    }

    DotNet.invokeMethodAsync(assembly, trackProgress, position(), false);
}, 100)

function initiate(buffer) {
    if (isAudioPlaying()) {
        dispose();
    }

    volumeNode = context.createGain();
    volumeNode.gain.value = currentVolume;
    volumeNode.connect(context.destination);

    sourceNode = context.createBufferSource();
    sourceNode.buffer = buffer;
    sourceNode.connect(volumeNode);
    sourceNode.start();
    startTime = context.currentTime;

    DotNet.invokeMethodAsync(assembly, trackStart, sourceNode.buffer.duration * 1000);
    sourceNode.onended = onEnded;
}

function resume() {
    context.resume();
}

function suspend() {
    context.suspend();
}

function onEnded() {
    dispose();
    DotNet.invokeMethodAsync(assembly, trackProgress, position(), true);
}

function position() {
    return ((context.currentTime - startTime) / sourceNode.playbackRate.value) * 1000;
}

function updateVolumeNode(volume) {
    currentVolume = volume / 100;
    volumeNode.gain.value = currentVolume;
}

function isAudioPlaying() {
    return ((sourceNode && sourceNode?.buffer.length > 0) ||
        (context.state !== "suspended" &&
        context.state !== "closed"));
}

function dispose() {
    sourceNode.stop();
    sourceNode.disconnect();
    sourceNode.onended = null;
}