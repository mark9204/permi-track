// src/services/apiClient.ts
import axios from 'axios';
import { API_URL } from '../config';

const apiClient = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request Interceptor: Token csatolása
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response Interceptor: 401 kezelése (Token Refresh)
apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        // Ha 401-et kapunk és még nem próbáltuk újra
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;
            const refreshToken = localStorage.getItem('refreshToken');

            if (refreshToken) {
                try {
                    // FONTOS: Itt egy teljesen új axios példányt használunk a frissítéshez,
                    // hogy elkerüljük a végtelen ciklust és a körkörös függőséget!
                    const response = await axios.post(`${API_URL}/auth/refresh`, { refreshToken });

                    const newAccessToken = response.data.accessToken;
                    const newRefreshToken = response.data.refreshToken; // Ha újat is ad a backend

                    // Mentés
                    localStorage.setItem('accessToken', newAccessToken);
                    if (newRefreshToken) {
                        localStorage.setItem('refreshToken', newRefreshToken);
                    }

                    // Header frissítése és az eredeti kérés ismétlése
                    apiClient.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`;
                    originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;

                    return apiClient(originalRequest);
                } catch (refreshError) {
                    // Ha a refresh is sikertelen (lejárt minden), logout
                    console.error("Token refresh failed", refreshError);
                    localStorage.clear();
                    window.location.href = '/login';
                    return Promise.reject(refreshError);
                }
            } else {
                // Ha nincs refresh token, logout
                localStorage.clear();
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);

export default apiClient;