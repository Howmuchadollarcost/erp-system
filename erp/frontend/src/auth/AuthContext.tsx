import { createContext, useContext, useEffect, useState } from 'react';
import axios from 'axios';

export type UserInfo = {
  userId: string;
  username: string;
  role: 'Admin' | 'Supervisor' | 'Worker';
  rank?: number | null;
};

type AuthContextType = {
  user: UserInfo | null;
  token: string | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType>({
  user: null,
  token: null,
  login: async () => {},
  logout: () => {},
});

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const saved = localStorage.getItem('auth');
    if (saved) {
      const parsed = JSON.parse(saved);
      setUser(parsed.user);
      setToken(parsed.token);
      axios.defaults.headers.common['Authorization'] = `Bearer ${parsed.token}`;
    }
  }, []);

  const login = async (username: string, password: string) => {
    const res = await axios.post('/api/auth/login', { username, password });
    const data = res.data as { token: string; role: UserInfo['role']; rank?: number | null; userId: string; username: string };
    const u: UserInfo = { userId: data.userId, username: data.username, role: data.role, rank: data.rank ?? null };
    setUser(u);
    setToken(data.token);
    localStorage.setItem('auth', JSON.stringify({ user: u, token: data.token }));
    axios.defaults.headers.common['Authorization'] = `Bearer ${data.token}`;
  };

  const logout = () => {
    setUser(null);
    setToken(null);
    localStorage.removeItem('auth');
    delete axios.defaults.headers.common['Authorization'];
  };

  return <AuthContext.Provider value={{ user, token, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}