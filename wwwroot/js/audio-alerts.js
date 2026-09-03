// Gestor de Audio Web con Limpieza Automática de Recursos
let audioCtx = null;
let newOrderInterval = null;

function getAudioContext() {
    if (!audioCtx) {
        audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    }
    if (audioCtx.state === 'suspended') {
        audioCtx.resume();
    }
    return audioCtx;
}

function playSoftChime(frequencies, durations, gainValue = 0.08) {
    try {
        const ctx = getAudioContext();

        // Si el contexto sigue suspendido por falta de gesto del usuario, salir
        if (ctx.state === 'suspended') {
            console.warn("AudioContext suspendido. Esperando interacción del usuario.");
            return;
        }

        let startTime = ctx.currentTime;

        frequencies.forEach((freq, index) => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq, startTime);

            gain.gain.setValueAtTime(0, startTime);
            gain.gain.linearRampToValueAtTime(gainValue, startTime + 0.05);
            gain.gain.exponentialRampToValueAtTime(0.0001, startTime + durations[index]);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(startTime);
            osc.stop(startTime + durations[index]);

            startTime += 0.12;
        });
    } catch (e) {
        console.warn("Audio Interop notice:", e);
    }
}

window.playNewOrderAlert = function () {
    playSoftChime([523.25, 659.25, 783.99], [0.4, 0.4, 0.6], 0.08);
};

window.startNewOrderLoop = function (intervalSeconds) {
    window.stopNewOrderLoop();
    window.playNewOrderAlert();
    newOrderInterval = setInterval(() => {
        window.playNewOrderAlert();
    }, intervalSeconds * 1000);
};

window.stopNewOrderLoop = function () {
    if (newOrderInterval) {
        clearInterval(newOrderInterval);
        newOrderInterval = null;
    }
};

window.playCriticalDelayAlert = function () {
    playSoftChime([440.00, 349.23], [0.5, 0.7], 0.07);
};

// Reanudar el contexto correcto al interactuar con cualquier parte de la pantalla
window.resumeAudioContext = function () {
    const ctx = getAudioContext();
    if (ctx && ctx.state === 'suspended') {
        ctx.resume().then(function () {
            console.log('AudioContext reanudado con éxito');
        }).catch(function (err) {
            console.error('Error al reanudar AudioContext:', err);
        });
    }
};

// Escuchar clics globales para desbloquear AudioContext
document.addEventListener('click', function () {
    window.resumeAudioContext();
});

window.addEventListener('pagehide', () => {
    window.stopNewOrderLoop();
});