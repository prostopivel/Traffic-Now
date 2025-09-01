create or replace function select_map(CurrentMapId uuid)
returns table(
	MapId uuid,
	MapName varchar(100)
) as $$
begin
	return query
	select m.Id, m.Name from Maps m
	where m.Id = CurrentMapId;
end;
$$ language plpgsql;

create or replace function create_map(
	MapId uuid,
	MapName varchar(100))
returns uuid as $$
begin
	insert into Maps (Id, Name)
	values (MapId, MapName);
	
	return MapId;
end;
$$ language plpgsql;

create or replace function update_map(
	MapId uuid,
	MapName varchar(100))
returns uuid as $$
begin
	update Maps
	set Name = MapName
	where Id = MapId;
	
	return MapId;
end;
$$ language plpgsql;

create or replace function delete_map(MapId uuid)
returns uuid as $$
begin
	delete from Maps
	where Id = MapId;
	
	return MapId;
end;
$$ language plpgsql;

create or replace function search_map(CurrentMapName varchar(100))
returns table (
	MapId uuid,
	MapName varchar(100)
) as $$
begin
	return query
	select m.Id, m.Name from Maps m
	where m.Name = CurrentMapName;
end;
$$ language plpgsql;

create or replace function select_user_maps(CurrentUserId uuid)
returns table (
    MapId uuid,
    MapName varchar(100)
) as $$
begin
    return query
    select m.Id, m.Name from Maps m
    join Users_Maps um on um.MapId = m.Id
    where um.UserId = CurrentUserId;
end;
$$ language plpgsql;

create or replace function add_user_map(CurrentUserId uuid, CurrentMapId uuid)
returns uuid as $$
begin
	insert into Users_Maps (UserId, MapId)
	values (CurrentUserId, CurrentMapId)
	on conflict (UserId, MapId) do nothing;

	return CurrentUserId;
end;
$$ language plpgsql;

select * from Users_Maps