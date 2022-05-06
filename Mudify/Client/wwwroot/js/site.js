let context;
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
    DotNet.invokeMethodAsync("Mudify.Client", "OnTrackProgress", progress);
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

function setupAudio() {
    AudioContext = window.AudioContext || window.webkitAudioContext;
    context = new AudioContext();
}

function initiate(buffer) {
    if (!hasEnded && source) {
        dispose();
    }

    alert(context);
    source = context.createBufferSource();
    source.buffer = buffer;
    source.connect(context.destination);
    source.start();
    start = context.currentTime;
    alert(source + " - " + source.buffer.length);
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
    DotNet.invokeMethodAsync("Mudify.Client", "OnTrackFinish");
}

function dispose() {
    hasEnded = true;
    source.stop();
    source.disconnect();
    source.onended = null;
}