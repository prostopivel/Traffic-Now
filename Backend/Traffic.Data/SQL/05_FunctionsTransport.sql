create or replace function select_transport(TransportId uuid)
returns table (
	Id uuid,
	UserId uuid,
	PointId uuid
) as $$
begin
	return query select * from Transport
	where Id = TransportId;
end;
$$ language plpgsql;

create or replace function create_transport(
	TransportId uuid,
	UserId uuid,
	PointId uuid)
returns uuid as $$
begin
	insert into Transport (Id, UserId, PointId)
	values (TransportId, UserId, PointId);

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
	return query select * from Transport t
	join User u on t.UserId = u.Id
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
	return query select * from select_user_transport(CurrentUserId) t
	join Point p on t.PointId = p.Id
	join Map m on p.MapId = m.Id
	where m.Id = CurrentMapId;
end;
$$ language plpgsql;










