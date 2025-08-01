// Token decode utility - Browser console'da çalıştırılabilir
function decodeToken(token) {
    try {
        const decoded = atob(token);
        console.log('Decoded Token:', decoded);
        return decoded;
    } catch (e) {
        console.error('Token decode error:', e);
    }
}

// Kullanım:
// const token = localStorage.getItem('accessToken');
// decodeToken(token);
