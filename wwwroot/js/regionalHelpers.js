window.regionalHelpers = {
    formatLocalTime: (raw) => {
        if (!raw) return "";

        // Como el formato siempre es el mismo, lo pasamos directo a Date
        const d = new Date(raw);
        if (isNaN(d.getTime())) return raw;

        const pad = n => String(n).padStart(2, '0');
        return `${pad(d.getDate())}/${pad(d.getMonth() + 1)} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }
};