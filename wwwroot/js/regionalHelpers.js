window.regionalHelpers = {
    // Convierte una fecha UTC (ISO 8601) a la hora local del cliente -> "dd/MM HH:mm"
    formatLocalTime: (raw) => {
        // La API entrega UTC; si el string no trae offset/Z, se asume UTC
        let iso = raw.trim().replace(' ', 'T');
        if (!/(Z|[+-]\d{2}:?\d{2})$/i.test(iso)) iso += 'Z';

        const d = new Date(iso);
        if (isNaN(d.getTime())) return raw;

        const pad = n => String(n).padStart(2, '0');
        return `${pad(d.getDate())}/${pad(d.getMonth() + 1)} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
    }
};