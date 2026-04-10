using System;
using System.Collections.Generic;
using System.Linq;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TSUI = Tekla.Structures.Model.UI;
using TeklaMcp.Contracts.Dtos;
using Tekla.Structures;
using TS = Tekla.Structures;
using Fusion;

namespace TeklaMcp.Server.Tekla
{
    public class TeklaModelFacade
    {
        private readonly Model _model;

        public TeklaModelFacade()
        {
            _model = new Model();
            if (!_model.GetConnectionStatus())
                throw new InvalidOperationException(
                    "Cannot connect to Tekla Structures. Ensure a model is open.");
        }

        public PlaceBeamResult InsertBeam(PlaceBeamArgs args)
        {
            var beam = BuildBeam(args);
            bool ok = beam.Insert();
            if (ok) _model.CommitChanges();

            return new PlaceBeamResult
            {
                Success = ok,
                Guid = beam.Identifier.GUID.ToString(),
                ObjectId = beam.Identifier.ID,
                Message = ok ? "Beam inserted successfully." : "Tekla Insert() returned false."
            };
        }

        public PlaceBeamsResult InsertBeams(PlaceBeamsArgs args)
        {
            var results = new List<PlaceBeamResult>();
            bool anyOk = false;

            foreach (var beamArgs in args.Beams)
            {
                var beam = BuildBeam(beamArgs);
                bool ok = beam.Insert();
                anyOk |= ok;
                results.Add(new PlaceBeamResult
                {
                    Success = ok,
                    Guid = beam.Identifier.GUID.ToString(),
                    ObjectId = beam.Identifier.ID,
                    Message = ok ? "Beam inserted successfully." : "Tekla Insert() returned false."
                });
            }

            if (anyOk) _model.CommitChanges();

            int succeeded = results.Count(r => r.Success);
            int failed = results.Count - succeeded;

            return new PlaceBeamsResult
            {
                Success = failed == 0,
                Message = $"Inserted {succeeded} of {results.Count} beams" + (failed > 0 ? $", {failed} failed." : "."),
                Results = results
            };
        }

        private Beam BuildBeam(PlaceBeamArgs args)
        {
            var beam = new Beam
            {
                StartPoint = new Point(args.Start.X, args.Start.Y, args.Start.Z),
                EndPoint = new Point(args.End.X, args.End.Y, args.End.Z)
            };

            beam.Profile.ProfileString = args.Profile;
            beam.Material.MaterialString = args.Material ?? "S235JR";
            beam.Class = (args.Class ?? 3).ToString();

            var pos = args.Position;
            beam.Position.Plane = pos?.Plane != null && Enum.TryParse(pos.Plane, out Position.PlaneEnum plane) ? plane : Position.PlaneEnum.MIDDLE;
            beam.Position.Depth = pos?.Depth != null && Enum.TryParse(pos.Depth, out Position.DepthEnum depth) ? depth : Position.DepthEnum.MIDDLE;
            beam.Position.Rotation = pos?.Rotation != null && Enum.TryParse(pos.Rotation, out Position.RotationEnum rotation) ? rotation : Position.RotationEnum.FRONT;
            if (pos?.PlaneOffset != null) beam.Position.PlaneOffset = pos.PlaneOffset.Value;
            if (pos?.DepthOffset != null) beam.Position.DepthOffset = pos.DepthOffset.Value;
            if (pos?.RotationOffset != null) beam.Position.RotationOffset = pos.RotationOffset.Value;

            if (args.Name != null) beam.Name = args.Name;

            return beam;
        }

        public ModifyBeamResult ModifyBeam(ModifyBeamArgs args)
        {
            var obj = _model.SelectModelObject(new Identifier(args.Guid));
            if (obj == null)
                return new ModifyBeamResult { Success = false, Message = "Object with specified GUID not found." };

            if (obj is not Beam beam)
                return new ModifyBeamResult { Success = false, Message = "Object is not a Beam." };

            if (args.Profile != null) beam.Profile.ProfileString = args.Profile;
            if (args.Material != null) beam.Material.MaterialString = args.Material;
            if (args.Class != null) beam.Class = args.Class.Value.ToString();
            if (args.Name != null) beam.Name = args.Name;
            if (args.Start != null) beam.StartPoint = new Point(args.Start.X, args.Start.Y, args.Start.Z);
            if (args.End != null) beam.EndPoint = new Point(args.End.X, args.End.Y, args.End.Z);

            var pos = args.Position;
            if (pos != null)
            {
                if (pos.Plane != null && Enum.TryParse(pos.Plane, out Position.PlaneEnum plane)) beam.Position.Plane = plane;
                if (pos.Depth != null && Enum.TryParse(pos.Depth, out Position.DepthEnum depth)) beam.Position.Depth = depth;
                if (pos.Rotation != null && Enum.TryParse(pos.Rotation, out Position.RotationEnum rotation)) beam.Position.Rotation = rotation;
                if (pos.PlaneOffset != null) beam.Position.PlaneOffset = pos.PlaneOffset.Value;
                if (pos.DepthOffset != null) beam.Position.DepthOffset = pos.DepthOffset.Value;
                if (pos.RotationOffset != null) beam.Position.RotationOffset = pos.RotationOffset.Value;
            }

            bool ok = beam.Modify();
            if (ok) _model.CommitChanges();

            return new ModifyBeamResult
            {
                Success = ok,
                Message = ok ? "Beam modified successfully." : "Tekla Modify() returned false."
            };
        }

        public PlaceColumnResult InsertColumn(PlaceColumnArgs args)
        {
            var column = new Beam
            {
                StartPoint = new Point(args.Base.X, args.Base.Y, args.Base.Z),
                EndPoint = new Point(args.Base.X, args.Base.Y, args.Base.Z + args.Height)
            };

            column.Profile.ProfileString = args.Profile;
            column.Material.MaterialString = args.Material ?? "S235JR";
            column.Class = (args.Class ?? 3).ToString();


            column.Position.Depth = Position.DepthEnum.BEHIND;
            column.Position.Rotation = Position.RotationEnum.FRONT;
            column.Position.Plane = Position.PlaneEnum.MIDDLE;


            if (args.Name != null) column.Name = args.Name;

            bool ok = column.Insert();
            if (ok) _model.CommitChanges();

            return new PlaceColumnResult
            {
                Success = ok,
                Guid = column.Identifier.GUID.ToString(),
                ObjectId = column.Identifier.ID,
                Message = ok ? "Column inserted successfully." : "Tekla Insert() returned false."
            };
        }

        public FilterModelResult FilterModel(FilterModelArgs args)
        {
            var objects = new List<ModelObjectDto>();
            var enumerator = _model.GetModelObjectSelector().GetAllObjects();
            var effectiveLimit = args.Limit.HasValue ? Math.Min(args.Limit.Value, 500) : 500;

            while (enumerator.MoveNext())
            {
                if (objects.Count >= effectiveLimit) break;

                var mo = enumerator.Current as ModelObject;
                if (mo == null) continue;

                bool typeMatch = args.Type == null || mo.GetType().Name == args.Type;

                if (!typeMatch) continue;

                if (mo is Part p)
                {
                    bool nameMatch = args.Name == null || p.Name == args.Name;
                    bool materialMatch = args.Material == null || p.Material.MaterialString == args.Material;
                    bool profileMatch = args.Profile == null || p.Profile.ProfileString == args.Profile;
                    bool classMatch = args.Class == null || p.Class == args.Class.ToString();

                    if (nameMatch && materialMatch && profileMatch && classMatch && Matches(mo, args))
                    {
                        objects.Add(ToDto(mo));
                    }
                }
                else if (Matches(mo, args))
                {
                    objects.Add(ToDto(mo));
                }
            }

            if (objects.Count == 0)
            {
                return new FilterModelResult
                {
                    Success = false,
                    Message = "No objects found matching criteria.",
                    Objects = new List<ModelObjectDto>()
                };
            }
            return new FilterModelResult
            {
                Success = true,
                Message = $"Found {objects.Count} objects matching criteria.",
                Objects = objects
            };

        }

        private bool Matches(ModelObject mo, FilterModelArgs args)
        {
            if (args.Type != null && !mo.GetType().Name.Contains(args.Type))
                return false;

            if (mo is Part part)
            {
                if (args.Name != null && !part.Name.Contains(args.Name)) return false;
                if (args.Profile != null && !part.Profile.ProfileString.Contains(args.Profile)) return false;
                if (args.Material != null && !part.Material.MaterialString.Contains(args.Material)) return false;
                if (args.Class != null && part.Class != args.Class.ToString()) return false;
            }

            if (args.UserDefinedAttributes != null)
            {
                foreach (var uda in args.UserDefinedAttributes)
                {
                    string value = "";
                    mo.GetUserProperty(uda.Key, ref value);
                    if (value != uda.Value) return false;
                }
            }

            return true;
        }

        private ModelObjectDto ToDto(ModelObject mo)
        {
            var dto = new ModelObjectDto
            {
                Guid = mo.Identifier.GUID.ToString(),
                Type = mo.GetType().Name,
            };

            if (mo is Part part)
            {
                dto.Name = part.Name;
                dto.Profile = part.Profile.ProfileString;
                dto.Material = part.Material.MaterialString;
                dto.Class = part.Class;
                dto.Finish = part.Finish;
                dto.CastUnitType = part.CastUnitType.ToString();

                dto.AssemblyNumber = new NumberingSeriesDto
                {
                    Prefix = part.AssemblyNumber.Prefix,
                    StartNumber = part.AssemblyNumber.StartNumber
                };

                dto.PartNumber = new NumberingSeriesDto
                {
                    Prefix = part.PartNumber.Prefix,
                    StartNumber = part.PartNumber.StartNumber
                };

                dto.Position = new PositionDto
                {
                    Plane = part.Position.Plane.ToString(),
                    PlaneOffset = part.Position.PlaneOffset,
                    Depth = part.Position.Depth.ToString(),
                    DepthOffset = part.Position.DepthOffset,
                    Rotation = part.Position.Rotation.ToString(),
                    RotationOffset = part.Position.RotationOffset
                };

                if (part is Beam beam)
                {
                    dto.StartPoint = new Point3 { X = beam.StartPoint.X, Y = beam.StartPoint.Y, Z = beam.StartPoint.Z };
                    dto.EndPoint = new Point3 { X = beam.EndPoint.X, Y = beam.EndPoint.Y, Z = beam.EndPoint.Z };
                }
            }

            return dto;
        }


        private BeamDto ToBeamDto(Beam beam)
        {
            var dto = new BeamDto
            {
                Guid = beam.Identifier.GUID.ToString(),
                Type = beam.GetType().Name,
            };


            dto.Name = beam.Name;
            dto.Profile = beam.Profile.ProfileString;
            dto.Material = beam.Material.MaterialString;
            dto.Class = beam.Class;
            dto.Finish = beam.Finish;
            dto.CastUnitType = beam.CastUnitType.ToString();

            dto.AssemblyNumber = new NumberingSeriesDto
            {
                Prefix = beam.AssemblyNumber.Prefix,
                StartNumber = beam.AssemblyNumber.StartNumber
            };

            dto.PartNumber = new NumberingSeriesDto
            {
                Prefix = beam.PartNumber.Prefix,
                StartNumber = beam.PartNumber.StartNumber
            };

            dto.Position = new PositionDto
            {
                Plane = beam.Position.Plane.ToString(),
                PlaneOffset = beam.Position.PlaneOffset,
                Depth = beam.Position.Depth.ToString(),
                DepthOffset = beam.Position.DepthOffset,
                Rotation = beam.Position.Rotation.ToString(),
                RotationOffset = beam.Position.RotationOffset
            };


            dto.StartPoint = new Point3 { X = beam.StartPoint.X, Y = beam.StartPoint.Y, Z = beam.StartPoint.Z };
            dto.EndPoint = new Point3 { X = beam.EndPoint.X, Y = beam.EndPoint.Y, Z = beam.EndPoint.Z };



            return dto;
        }

        public DeleteObjectsResult DeleteObjects(DeleteObjectsArgs args)
        {
            var results = new System.Collections.Generic.List<DeleteObjectItemResult>();

            foreach (var guid in args.Guids)
            {
                var singleResult = DeleteObject(new DeleteObjectArgs { Guid = guid });
                results.Add(new DeleteObjectItemResult
                {
                    Guid = guid,
                    Success = singleResult.Success,
                    Message = singleResult.Message
                });
            }

            int succeeded = results.Count(r => r.Success);
            int failed = results.Count - succeeded;

            return new DeleteObjectsResult
            {
                Success = failed == 0,
                Message = $"Deleted {succeeded} of {results.Count} objects" + (failed > 0 ? $", {failed} failed." : "."),
                Results = results
            };
        }

        public DeleteObjectResult DeleteObject(DeleteObjectArgs args)
        {
            if (args.Guid != null)
            {
                var guid = new Identifier(args.Guid);
                var obj = _model.SelectModelObject(guid);
                if (obj != null)
                {
                    bool ok = obj.Delete();
                    if (ok) _model.CommitChanges();
                    return new DeleteObjectResult
                    {
                        Success = ok,
                        Message = ok ? "Object deleted successfully." : "Tekla Delete() returned false."
                    };
                }
                else
                {
                    return new DeleteObjectResult
                    {
                        Success = false,
                        Message = "Object with specified GUID not found."
                    };
                }
            }
            else
            {
                return new DeleteObjectResult
                {
                    Success = false,
                    Message = "GUID must be provided to delete an object."
                };
            }

        }

        public GetUserSelectionResult GetUserSelection()
        {
            var objects = new List<ModelObjectDto>();
            try
            {
                var selectedobjects = new TSUI.ModelObjectSelector().GetSelectedObjects();

                while (selectedobjects.MoveNext())
                {
                    var mo = selectedobjects.Current as ModelObject;
                    if (mo == null) continue;
                    objects.Add(ToDto(mo));
                }

                return new GetUserSelectionResult
                {
                    Success = objects.Count > 0,
                    Message = objects.Count > 0 ? $"Selected {objects.Count} objects." : "No objects selected.",
                    Objects = objects
                };
            }
            catch (Exception ex)
            {
                return new GetUserSelectionResult
                {
                    Success = false,
                    Message = "Error retrieving selected objects." + ex.Message,
                    Objects = new List<ModelObjectDto>()
                };
            }

        }

        public UserSelectedBeamResult GetInfoFromUserSelectedBeam()
        {
            var beam = new Beam();
            var picker = new TSUI.Picker();
            var selectedObject = picker.PickObject(TSUI.Picker.PickObjectEnum.PICK_ONE_PART, "Select a beam");
            if (selectedObject is Beam b)
            {
                return new UserSelectedBeamResult
                {
                    Success = true,
                    Message = "Selected beam found",
                    Beam = ToBeamDto(b)

                };
            }

            return new UserSelectedBeamResult
            {
                Success = false,
                Message = "Selected object not found",
                Beam = null
            };

        }
        public List<string> GetModelObjectNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var objects = TS.TeklaStructures.Model.AllObjects.OfType<Part>().ToList();
            names.AddRange(objects.Where(p => !string.IsNullOrEmpty(p.Name)).Select(p => p.Name));
            return names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public OverlappingNumberingResult GetOverlappingNumbering(OverlappingNumberingArgs args)
        {
            var checkType = (args.CheckType ?? "both").ToLowerInvariant();
            var effectiveLimit = args.Limit.HasValue ? Math.Min(args.Limit.Value, 1000) : 200;

            // key: mark string → list of parts
            var castunitnumbers = new Dictionary<string, List<Part>>(StringComparer.Ordinal);
            var unqiquenumbers = new Dictionary<string, List<Part>>(StringComparer.Ordinal);

            int totalScanned = 0;
            var enumerator = _model.GetModelObjectSelector().GetAllObjects();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not Part part) continue;
                totalScanned++;

                if (checkType == "part" || checkType == "both")
                {
                    var mark = AssemblyPos(part);

                    if (!string.IsNullOrWhiteSpace(mark))
                    {
                        if (!castunitnumbers.TryGetValue(mark, out var list))
                            castunitnumbers[mark] = list = new List<Part>();
                        list.Add(part);
                    }
                }

                if (checkType == "assembly" || checkType == "both")
                {
                    var mark = Acn(part);
                    if (!string.IsNullOrWhiteSpace(mark))
                    {
                        if (!unqiquenumbers.TryGetValue(mark, out var list))
                            unqiquenumbers[mark] = list = new List<Part>();
                        list.Add(part);
                    }
                }
            }

            var groupsCU = new List<OverlapGroup>();
            var groups = new List<OverlapGroup>();

            foreach (var kv in castunitnumbers)
            {
                if (kv.Value.Count < 2) continue;
                if (groupsCU.Count >= effectiveLimit) break;
                groupsCU.Add(BuildGroup(kv.Key, "part", kv.Value));
            }

            foreach (var kv in unqiquenumbers)
            {
                if (kv.Value.Count < 2) continue;
                if (groups.Count >= effectiveLimit) break;
                groups.Add(BuildGroup(kv.Key, "assembly", kv.Value));
            }

            bool hasOverlapsCU = groups.Count > 0;
            bool hasOverlaps = groups.Count > 0;
            string message = "";
            if (hasOverlapsCU)
            {
                message = $"Found {groups.Count} overlapping Cast Unit Positions across {totalScanned} parts.";
            }
            if (hasOverlaps)
            {
                if (message != "")
                {
                    message += Environment.NewLine;
                }
                message += $"Found {groups.Count} overlapping Unique numbers across {totalScanned} parts.";

            }

            return new OverlappingNumberingResult
            {
                Success = true,
                Message = hasOverlaps
                    ? $"Found {groups.Count} overlapping numbering group(s) across {totalScanned} parts."
                    : $"No overlapping numbering found across {totalScanned} parts.",
                TotalPartsScanned = totalScanned,
                OverlapGroupCount = groups.Count,
                OverlapGroups = groups
            };
        }


        public string AssemblyPos(Part b)
        {
            string possie = "";
            b.GetReportProperty("CAST_UNIT_POS", ref possie);
            return possie;
        }
        private string Acn(Part b)
        {
            string acn = "";
            b.GetUserProperty("ACN", ref acn);
            return acn;
        }

        private static OverlapGroup BuildGroup(string mark, string numberType, List<Part> parts)
        {
            var group = new OverlapGroup { Mark = mark, NumberType = numberType };
            foreach (var p in parts)
            {
                group.Guids.Add(p.Identifier.GUID.ToString());
                group.Parts.Add(new OverlapPartInfo
                {
                    Guid = p.Identifier.GUID.ToString(),
                    Name = p.Name ?? "",
                    Profile = p.Profile.ProfileString ?? "",
                    Material = p.Material.MaterialString ?? "",
                    Type = p.GetType().Name
                });
            }
            return group;
        }

        public List<GridDto> GetAllGrids()
        {
            var grids = new List<GridDto>();
            var enumerator = _model.GetModelObjectSelector().GetAllObjectsWithType(ModelObject.ModelObjectEnum.GRID);

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not Grid grid) continue;

                grids.Add(new GridDto
                {
                    Guid = grid.Identifier.GUID.ToString(),
                    Name = grid.Name ?? "",
                    CoordinateX = grid.CoordinateX ?? "",
                    CoordinateY = grid.CoordinateY ?? "",
                    CoordinateZ = grid.CoordinateZ ?? "",
                    LabelX = grid.LabelX ?? "",
                    LabelY = grid.LabelY ?? "",
                    LabelZ = grid.LabelZ ?? ""
                });
            }

            return grids;
        }


    }
}
