create or replace function select_map(MapId uuid)
returns table(
	Id uuid,
	Name varchar(100)
) as $$
begin
	return query select * from Maps m
	where m.Id = MapId;
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









