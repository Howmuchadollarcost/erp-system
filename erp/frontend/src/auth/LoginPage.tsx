import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, Container, Paper, Stack, TextField, Typography } from '@mui/material';
import { useAuth } from './AuthContext';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await login(username, password);
      navigate('/');
    } catch (err) {
      setError('Invalid credentials');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xs" sx={{ mt: 12 }}>
      <Paper sx={{ p: 3 }}>
        <Typography variant="h5" gutterBottom>ERP Login</Typography>
        <form onSubmit={handleSubmit}>
          <Stack spacing={2}>
            <TextField label="Username" value={username} onChange={(e) => setUsername(e.target.value)} fullWidth required />
            <TextField label="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} fullWidth required />
            {error && <Typography color="error">{error}</Typography>}
            <Button type="submit" variant="contained" disabled={loading}>Login</Button>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}