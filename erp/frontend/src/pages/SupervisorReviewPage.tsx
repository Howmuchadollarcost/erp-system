import { useEffect, useState } from 'react';
import axios from 'axios';
import { Button, Container, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography } from '@mui/material';

export default function SupervisorReviewPage() {
  const [query, setQuery] = useState('');
  const [items, setItems] = useState<any[]>([]);
  const [year, setYear] = useState<number | ''>('');
  const [week, setWeek] = useState<number | ''>('');

  const load = async () => {
    const params = new URLSearchParams();
    if (query) params.append('username', query);
    if (year) params.append('year', String(year));
    if (week) params.append('week', String(week));
    const res = await axios.get(`/api/timesheets/review?${params.toString()}`);
    setItems(res.data);
  };

  useEffect(() => {
    load();
  }, []);

  const act = async (id: string, approve: boolean) => {
    await axios.post('/api/timesheets/review/action', { timesheetId: id, approve });
    await load();
  };

  return (
    <Container sx={{ mt: 3 }}>
      <Paper sx={{ p: 2, mb: 2 }}>
        <Stack direction="row" spacing={2}>
          <TextField label="Search username" value={query} onChange={(e) => setQuery(e.target.value)} />
          <TextField label="Year" value={year} type="number" onChange={(e) => setYear(e.target.value ? Number(e.target.value) : '')} />
          <TextField label="Week" value={week} type="number" onChange={(e) => setWeek(e.target.value ? Number(e.target.value) : '')} />
          <Button variant="contained" onClick={load}>Search</Button>
        </Stack>
      </Paper>
      <Paper sx={{ p: 2 }}>
        <Typography variant="subtitle1" gutterBottom>Submissions</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>User</TableCell>
              <TableCell>Year</TableCell>
              <TableCell>Week</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {items.map((it) => (
              <TableRow key={it.id}>
                <TableCell>{it.user?.username || ''}</TableCell>
                <TableCell>{it.year}</TableCell>
                <TableCell>{it.week}</TableCell>
                <TableCell>{it.status}</TableCell>
                <TableCell>
                  <Stack direction="row" spacing={1}>
                    <Button size="small" onClick={() => act(it.id, true)} disabled={it.status === 'Approved'}>Approve</Button>
                    <Button size="small" color="error" onClick={() => act(it.id, false)} disabled={it.status === 'Declined'}>Decline</Button>
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Container>
  );
}