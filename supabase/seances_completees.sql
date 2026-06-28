create table if not exists public.seances_completees (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  programme_id uuid not null,
  semaine_index integer not null,
  jour_index integer not null,
  completed_at timestamptz not null default now(),
  unique (user_id, programme_id, semaine_index, jour_index)
);

alter table public.seances_completees enable row level security;

create policy "Users can read their completed sessions"
  on public.seances_completees
  for select
  using (auth.uid() = user_id);

create policy "Users can insert their completed sessions"
  on public.seances_completees
  for insert
  with check (auth.uid() = user_id);

create policy "Users can delete their completed sessions"
  on public.seances_completees
  for delete
  using (auth.uid() = user_id);
