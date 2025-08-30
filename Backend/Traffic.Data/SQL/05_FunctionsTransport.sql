create or replace function select_transport(TransportId uuid)
returns table (
	Id uuid,
	UserId uuid,
	PointId uuid
) as $$
begin
	return query
	select t.Id, t.UserId, t.PointId from Transport t
	where t.Id = TransportId;
end;
$$ language plpgsql;

create or replace function create_transport(
	TransportId uuid,
	TransportUserId uuid,
	TransportPointId uuid)
returns uuid as $$
begin
	insert into Transport (Id, UserId, PointId)
	values (TransportId, TransportUserId, TransportPointId);

	return TransportId;
end;
$$ language plpgsql;

create or replace function update_transport(
	TransportId uuid,
	UserId uuid,
	TransportPointId uuid)
returns uuid as $$
begin
	update Transport
	set PointId = TransportPointId
	where Id = TransportId;
	
	return TransportId;
end;
$$ language plpgsql;

create or replace function delete_transport(TransportId uuid)
returns uuid as $$
begin
	delete from Transport
	where Id = TransportId;
	
	return TransportId;
end;
$$ language plpgsql;

create or replace function select_user_transport(CurrentUserId uuid)
returns table (
	Id uuid,
	UserId uuid,
	PointId uuid
) as $$
begin
	return query
	select t.Id, t.UserId, t.PointId from Transport t
	join Users u on t.UserId = u.Id
	where u.Id = CurrentUserId;
end;
$$ language plpgsql;

create or replace function select_map_user_transport(CurrentMapId uuid, CurrentUserId uuid)
returns table (
	Id uuid,
	UserId uuid,
	PointId uuid
) as $$
begin
	return query
	select t.Id, t.UserId, t.PointId from select_user_transport(CurrentUserId) t
	join Points p on t.PointId = p.Id
	join Maps m on p.MapId = m.Id
	where m.Id = CurrentMapId;
end;
$$ language plpgsql;