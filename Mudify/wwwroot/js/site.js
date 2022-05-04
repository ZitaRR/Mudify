AudioContext = window.AudioContext || window.webkitAudioContext;
const context = new AudioContext();
const source = context.createBufferSource();

setInterval(() => {
    if (source.buffer.length <= 0) {
        return;
    }
    DotNet.invokeMethodAsync("Mudify", "OnTrackProgress");
}, 100)

window.buffer = (track) => {
    let array = new Uint8Array(track.audio);
    context.decodeAudioData(array.buffer, play);
}

function play(buffer) {
    source.buffer = buffer;
    source.connect(context.destination);
    source.start();

    DotNet.invokeMethodAsync("Mudify", "OnTrackStart");
    source.onended = onEnded;
}

function onEnded() {
    DotNet.invokeMethodAsync("Mudify", "OnTrackEnd");
}