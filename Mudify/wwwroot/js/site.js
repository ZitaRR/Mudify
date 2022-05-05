AudioContext = window.AudioContext || window.webkitAudioContext;
const context = new AudioContext();
let source;
let start = 0;
let hasEnded = true;

setInterval(() => {
    if (!source?.buffer || hasEnded ||
        context.state === "suspended" ||
        context.state === "closed") {
        return;
    }
    else if (source.buffer.length <= 0) {
        return;
    }

    progress = Math.round(((context.currentTime - start) / source.playbackRate.value) * 1000);
    DotNet.invokeMethodAsync("Mudify", "OnTrackProgress", progress);
}, 100)

window.play = (track) => {
    let array = new Uint8Array(track.audio);
    context.decodeAudioData(array.buffer, initiate);
}

window.pause = () => {
    suspend();
};

window.unpause = () => {
    resume();
}

function initiate(buffer) {
    if (!hasEnded && source) {
        source.disconnect();
    }

    source = context.createBufferSource();
    source.buffer = buffer;
    source.connect(context.destination);
    source.start();
    start = context.currentTime;
    hasEnded = false;

    DotNet.invokeMethodAsync("Mudify", "OnTrackStart");
    source.onended = onEnded;
}

function resume() {
    context.resume();
}

function suspend() {
    context.suspend();
}

function onEnded() {
    hasEnded = true;
    DotNet.invokeMethodAsync("Mudify", "OnTrackEnd");
}