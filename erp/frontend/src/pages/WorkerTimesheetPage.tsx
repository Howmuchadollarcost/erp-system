import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import axios from 'axios';
import { addWeeks, format, startOfWeek } from 'date-fns';
import { Button, Container, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, TextField, Typography } from '@mui/material';

type Row = {
  id?: string;
  projectOrTask: string;
  notes?: string;
  monday: number; tuesday: number; wednesday: number; thursday: number; friday: number; saturday: number; sunday: number;
};

export default function WorkerTimesheetPage() {
  const { user, logout } = useAuth();
  const [weekStart, setWeekStart] = useState(startOfWeek(new Date(), { weekStartsOn: 1 }));
  const [year, setYear] = useState<number>(new Date().getFullYear());
  const [week, setWeek] = useState<number>(() => {
    const onejan = new Date(new Date().getFullYear(), 0, 1);
    const diff = (weekStart.getTime() - onejan.getTime()) / 86400000;
    return Math.ceil((diff + onejan.getDay() + 1) / 7);
  });
  const [rows, setRows] = useState<Row[]>([]);
  const [status, setStatus] = useState<string>('Draft');
  const [history, setHistory] = useState<any[]>([]);

  const weekLabel = useMemo(() => `${year} - W${week} (${format(weekStart, 'MMM d')})`, [year, week, weekStart]);

  const loadTimesheet = async () => {
    const res = await axios.get(`/api/timesheets/mine?year=${year}&week=${week}`);
    const data = (res.data as any[])[0];
    if (data) {
      setRows((data.rows || []).map((r: any) => ({
        id: r.id,
        projectOrTask: r.projectOrTask,
        notes: r.notes || '',
        monday: r.monday || 0, tuesday: r.tuesday || 0, wednesday: r.wednesday || 0, thursday: r.thursday || 0, friday: r.friday || 0, saturday: r.saturday || 0, sunday: r.sunday || 0,
      })));
      setStatus(data.status);
    } else {
      setRows([]);
      setStatus('Draft');
    }
  };

  const loadHistory = async () => {
    const res = await axios.get(`/api/timesheets/mine`);
    setHistory(res.data);
  };

  useEffect(() => {
    loadTimesheet();
  }, [year, week]);

  useEffect(() => {
    loadHistory();
  }, []);

  const addRow = () => setRows([...rows, { projectOrTask: '', notes: '', monday: 0, tuesday: 0, wednesday: 0, thursday: 0, friday: 0, saturday: 0, sunday: 0 }]);
  const removeRow = (idx: number) => setRows(rows.filter((_, i) => i !== idx));

  const save = async (asSubmitted?: boolean) => {
    await axios.post('/api/timesheets/upsert', {
      year, week,
      rows: rows.map(r => ({ id: r.id, projectOrTask: r.projectOrTask, notes: r.notes, monday: r.monday, tuesday: r.tuesday, wednesday: r.wednesday, thursday: r.thursday, friday: r.friday, saturday: r.saturday, sunday: r.sunday })),
      status: asSubmitted ? 'Submitted' : 'Draft'
    });
    await loadTimesheet();
    await loadHistory();
  };

  const changeWeek = (delta: number) => {
    const next = addWeeks(weekStart, delta);
    setWeekStart(next);
    const onejan = new Date(next.getFullYear(), 0, 1);
    const diff = (next.getTime() - onejan.getTime()) / 86400000;
    setYear(next.getFullYear());
    setWeek(Math.ceil((diff + onejan.getDay() + 1) / 7));
  };

  return (
    <Container sx={{ mt: 3 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
        <Typography variant="h6">Timesheet - {weekLabel}</Typography>
        <Stack direction="row" spacing={1}>
          <Button onClick={() => changeWeek(-1)}>Prev week</Button>
          <Button onClick={() => changeWeek(1)}>Next week</Button>
          <Button variant="outlined" onClick={logout}>Logout</Button>
        </Stack>
      </Stack>
      <Paper sx={{ p: 2, mb: 2 }}>
        <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 2 }}>
          <Typography>Status: {status}</Typography>
          {user?.role === 'Supervisor' && <Button href="/review" variant="text">Review</Button>}
        </Stack>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Project/Task</TableCell>
              <TableCell>Mon</TableCell>
              <TableCell>Tue</TableCell>
              <TableCell>Wed</TableCell>
              <TableCell>Thu</TableCell>
              <TableCell>Fri</TableCell>
              <TableCell>Sat</TableCell>
              <TableCell>Sun</TableCell>
              <TableCell>Notes</TableCell>
              <TableCell></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map((r, idx) => (
              <TableRow key={idx}>
                <TableCell>
                  <TextField value={r.projectOrTask} onChange={(e) => setRows(rows.map((x, i) => i===idx?{...x, projectOrTask: e.target.value}:x))} size="small" />
                </TableCell>
                {(['monday','tuesday','wednesday','thursday','friday','saturday','sunday'] as const).map((d) => (
                  <TableCell key={d}>
                    <TextField type="number" inputProps={{ step: 0.25 }} value={r[d]}
                      onChange={(e) => setRows(rows.map((x, i) => i===idx?{...x, [d]: Number(e.target.value)}:x))}
                      size="small" />
                  </TableCell>
                ))}
                <TableCell>
                  <TextField value={r.notes} onChange={(e) => setRows(rows.map((x, i) => i===idx?{...x, notes: e.target.value}:x))} size="small" />
                </TableCell>
                <TableCell>
                  <Button color="error" onClick={() => removeRow(idx)}>Remove</Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
          <Button onClick={addRow}>Add row</Button>
          <Button variant="contained" onClick={() => save(false)}>Save Draft</Button>
          <Button variant="outlined" onClick={() => save(true)}>Submit</Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 2 }}>
        <Typography variant="subtitle1" gutterBottom>My submissions</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Year</TableCell>
              <TableCell>Week</TableCell>
              <TableCell>Status</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {history.map((h) => (
              <TableRow key={h.id}>
                <TableCell>{h.year}</TableCell>
                <TableCell>{h.week}</TableCell>
                <TableCell>{h.status}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Container>
  );
}